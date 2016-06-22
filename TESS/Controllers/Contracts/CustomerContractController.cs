using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Extensions;
using System.Data;
namespace TietoCRM.Controllers.Contracts
{
    public static class Distinct
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
    public class CustomerContractController : Controller
    {
        //Request["selected-contract"]
        // GET: CustomerContract
        public ActionResult Index()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            else
                this.ViewData.Add("Customers", view_Customer.getCustomerNames());
            this.ViewData.Add("ControllerName", "CustomerContract");
            this.ViewData.Add("PrimaryKey", "SSMA_timestamp");
            String on;
            if (ViewBag.Customers.Count <= 0)
                on = "";
            else
                on = ViewBag.Customers[0];
            if (Request["selected-contract-id"] != null && Request["selected-contract-id"] != "")
            {
                String id = Request["selected-contract-id"];
                view_Contract contract = new view_Contract("Contract_id = " + id);
                on = contract.Customer;
            }

            this.ViewData.Add("OfferNumber", on);

            this.ViewData.Add("Properties", typeof(view_Contract).GetProperties());
            this.ViewData.Add("Statuses", GetStatuses());
            this.ViewData.Add("ContractTypes", GetContractTypes());
            this.ViewData["Title"] = "Customer Contract";

            if (Request.QueryString["customer"] == null || Request.QueryString["customer"] == "")
                ViewData.Add("Customer", on);
            else
                ViewData.Add("Customer", Request["customer"]);
            
            return View();
        }

        public ActionResult ViewPdf()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            this.GenerateThings();

            string contractRequest = Request["contract-section"];
            if (!String.IsNullOrEmpty(contractRequest))
            {
                if (contractRequest == "_ModuleSection")
                    return View(contractRequest);
                else if (contractRequest == "_OldModuleSection")
                    return View(contractRequest);
                else if (contractRequest == "_HeaderSection")
                    return View(contractRequest);
                else if (contractRequest == "_SigningSection")
                    return View(contractRequest);
                else if (contractRequest == "_ModuleTerminationSection")
                    return View(contractRequest);
                else
                    return View();
            }
            else
                return View();
        }

        private void GenerateThings()
        {
           // string request = Request["selected-contract"];
            String urlCustomer = Request["customer"];
            String urlContractId = Request["contract-id"];
            bool ctrResign = false;

            view_Contract contract = new view_Contract("Contract_id = '" + urlContractId + "' AND Customer = '" + urlCustomer + "'");
            ViewData.Add("CustomerContract", contract);

            view_ContractText text = new view_ContractText();
            text.Select("Contract_id = '" + contract.Contract_id + "' AND Customer = '" + contract.Customer + "'");
            ViewData.Add("ContractText", text);

            ViewData.Add("Systems", GetAllSystemNames());
            ViewData.Add("MainCtrResign", GetMainContractsToResign(urlCustomer));

            view_ContractTemplate contractTemplate = new view_ContractTemplate();
            if(contract.Is(ContractType.MainContract))
            {
                contractTemplate.Select("Contract_id = '" + contract.Contract_id + "' AND Customer = '" + contract.Customer + "'");
            }
            else
            {
                contractTemplate.Select("Contract_id = '" + contract.Main_contract_id + "' AND Customer = '" + contract.Customer + "'");
            }
            
            ViewData.Add("ContractTemplate", contractTemplate);

            view_ContractHead contractHead = new view_ContractHead();
            contractHead.Select("Contract_id = '" + contract.Contract_id + "' AND Customer = '" + contract.Customer + "'");
            ViewData.Add("ContractHead", contractHead);

            HashSet<view_ContractRow> customersModules = new HashSet<view_ContractRow>();
            HashSet<view_ContractConsultantRow> customersServices = new HashSet<view_ContractConsultantRow>();

            List<view_Contract> asd = view_Contract.GetContracts(urlCustomer).Where(c => c.Status == "Giltigt").ToList();

            foreach (view_Contract validContract in view_Contract.GetContracts(urlCustomer).Where(c => c.Status == "Giltigt"))
            {
                customersModules = new HashSet<view_ContractRow>(customersModules.Concat(validContract._ContractRows));
                customersServices = new HashSet<view_ContractConsultantRow>(customersServices.Concat(validContract._ContractConsultantRows));
            }

            // Already existing modules and services from former contracts
            ViewData.Add("CustomersModules", customersModules);
            ViewData.Add("CustomersServices", customersServices);

            List<dynamic> remArticles = new List<dynamic>();
            List<dynamic> remEducationPortals = new List<dynamic>();
            List<dynamic> oldArticles = new List<dynamic>();
            List<dynamic> oldEducationPortals = new List<dynamic>();
            List<dynamic> articles = new List<dynamic>();
            List<dynamic> educationPortals = new List<dynamic>();

            foreach (view_ContractRow contractRow in contract._ContractRows)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + contractRow.Article_number);
                dynamic contractInfo = new ExpandoObject();
                contractInfo.Article_number = module.Article_number;
                contractInfo.Contract_id = contractRow.Contract_id;
                if(contractRow.Alias == null || contractRow.Alias == "")
                    contractInfo.Module = module.Module;
                else
                    contractInfo.Module = contractRow.Alias;
                contractInfo.System = module.System;
                contractInfo.Classification = module.Classification;
                contractInfo.License = contractRow.License;
                contractInfo.Maintenance = contractRow.Maintenance;
                contractInfo.Sortnr = contractRow.Sortnr;
                if (contractRow.Rewritten == true) ctrResign = true;

                if (module.System != "Lärportal")
                {
                    if (contractRow.Rewritten == true && contractRow.Removed == false) oldArticles.Add(contractInfo);
                    if (contractRow.Rewritten == true && contractRow.Removed == true) remArticles.Add(contractInfo);
                    if (contractRow.Rewritten == false) articles.Add(contractInfo);
                }
                else
                {
                    if (contractRow.Rewritten == true && contractRow.Removed == false) oldEducationPortals.Add(contractInfo);
                    if (contractRow.Rewritten == true && contractRow.Removed == true) remEducationPortals.Add(contractInfo);
                    if (contractRow.Rewritten == false) educationPortals.Add(contractInfo);
                }
            }

            oldArticles = oldArticles.OrderBy(a => a.Sortnr).ThenBy(a => a.Article_number).ToList();
            oldEducationPortals = oldEducationPortals.OrderBy(a => a.Article_number).ToList();
            remEducationPortals = remEducationPortals.OrderBy(a => a.Article_number).ToList();

            ViewData.Add("OldEducationPortals", oldEducationPortals);
            ViewData.Add("OldArticles", oldArticles);
            ViewData.Add("RemEducationPortals", remEducationPortals);
            ViewData.Add("CtrResign", ctrResign);

            articles = articles.OrderBy(a => a.Sortnr).ThenBy(a => a.Article_number).ToList();
            remArticles = remArticles.OrderBy(a => a.Sortnr).ThenBy(a => a.Article_number).ToList();
            educationPortals = educationPortals.OrderBy(a => a.Article_number).ToList();

            ViewData.Add("EducationPortals", educationPortals);
            ViewData.Add("Articles", articles);
            ViewData.Add("RemArticles", remArticles);

            List<dynamic> eduOptions = new List<dynamic>();
            List<dynamic> options = new List<dynamic>();

            foreach (view_ContractOption option in view_ContractOption.getAllOptions(urlContractId, urlCustomer))
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + option.Article_number);
                dynamic article = new ExpandoObject();
                article.Article_number = module.Article_number;
                article.Contract_id = option.Contract_id;
                article.Article = module.Module;
                article.System = module.System;
                article.Classification = module.Classification;
                article.License = option.License;
                article.Maintenance = option.Maintenance;

                if(module.System == "Lärportal")
                {
                    eduOptions.Add(article);
                }
                else
                {
                    options.Add(article);
                }
            }

            ViewData.Add("Options", options);
            ViewData.Add("EducationalOptions", eduOptions);

            view_CustomerContact cc = new view_CustomerContact();
            cc.Select("Customer = '" + contract.Customer + "' AND Contact_person = '" + contract.Contact_person + "'");
            ViewData.Add("CustomerContact", cc);

            view_Customer customer = new view_Customer();
            customer.Select("Customer = '" + contract.Customer + "'");
            ViewData.Add("Customer", customer);

            view_User user = new view_User();
            user.Select("Sign = '" + contract.Sign + "'");
            ViewData.Add("Representative", user);

            if (user.City != null)
            {
                int iSplit = 0;
                for (int i = 0; i < user.City.Length; i++)
                {
                    if (!char.IsWhiteSpace(user.City[i]) && !char.IsDigit(user.City[i]))
                    {
                        iSplit = i;
                        break;
                    }
                }

                ViewData.Add("OurCity", user.City.Substring(iSplit));
            }
            else
            {
                ViewData.Add("OurCity", "");
            }
            
            this.ViewData.Add("Services", view_Service.getAllServices());

            List<view_CustomerOffer> openOffers = view_CustomerOffer.getAllCustomerOffers(customer.Customer).Where(o => o.Offer_status == "Öppen").ToList();

            List<view_OfferRow> modules = new List<view_OfferRow>();
            List<view_ConsultantRow> services = new List<view_ConsultantRow>();

            foreach (view_CustomerOffer offer in openOffers)
            {
                foreach (view_OfferRow offerRow in offer._OfferRows)
                {
                    if (!modules.Contains(offerRow))
                        modules.Add(offerRow);
                }
                foreach (view_ConsultantRow consultantRow in offer._ConsultantRows)
                {
                    if (!services.Contains(consultantRow))
                        services.Add(consultantRow);
                }
            }

            ViewData.Add("OpenOfferModules", modules);
            ViewData.Add("OpenOfferServices", services);
            ViewData.Add("ActiveContracts", view_Contract.GetContracts(customer.Customer).Where(c => c.Status == "Giltigt"));

            this.ViewData["Title"] = "Customer Contract";

            //lägg till rätt properties beroende på typ., signs, statuse.
            List<System.Reflection.PropertyInfo> properties;
            if (contract.Status == "Sänt")
            {
                properties = typeof(view_Contract).GetProperties().Where(p => p.Name == "Contract_id" || p.Name == "Status" || p.Name == "Main_contract_id" || p.Name == "Contract_type" ||
                                                                     p.Name == "Term_of_notice" || p.Name == "Status" || p.Name == "Valid_from" || p.Name == "Valid_through" ||
                                                                     p.Name == "Extension" || p.Name == "Expire" || p.Name == "Observation" || p.Name == "Note" ||
                                                                     p.Name == "Sign").ToList();
            }
            else
            {
                properties = typeof(view_Contract).GetProperties().Where(p => p.Name == "Contract_id" || p.Name == "Status" || p.Name == "Observation" || p.Name == "Sign").ToList();
            }
            this.ViewData.Add("TableItems", properties);
            this.ViewData.Add("Statuses", GetStatuses());
            this.ViewData.Add("ContractTypes", GetContractTypes());
            List<String> mainContracts = view_Contract.GetContracts(customer.Customer).Where(c => c.Contract_type == "Huvudavtal").Select(c => c.Contract_id).ToList();
            this.ViewData.Add("MainContracts", mainContracts);
            this.ViewData.Add("Users", view_User.getAllUsers().Select(u => u.Sign));

            ViewData.Add("MainContractTitle", this.GetTopHead(urlCustomer, urlContractId));
            ViewData.Add("Prolog", this.GetProlog(urlCustomer, urlContractId));
            ViewData.Add("Epilog", this.GetEpilog(urlCustomer, urlContractId));
            ViewData.Add("ModuleText", this.GetModuleText(urlCustomer, urlContractId));

        }

        public ActionResult Pdf()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            String urlCustomer = Request["customer"];
            String urlContractId = Request["contract-id"];

            this.GenerateThings();

            String footerPath = Server.MapPath("~/Views/Shared/Footer_" + System.Web.HttpContext.Current.GetUser().Sign + ".html").Replace("\\", "/");
            String footerFilePath = "file:///" + footerPath;
            String headerFilePath = "file:///" + GenerateStaticHeader(/*Request["selected-contract"].ToString()*/);
            view_User user = ViewBag.Representative;

            FileStream ffs = updateFooter(footerPath, user);

            string cusomtSwitches = string.Format("--print-media-type --header-spacing 4 --header-html \"{1}\" --footer-html \"{0}\" ", footerFilePath, headerFilePath);
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = cusomtSwitches;
           
            // Set title and file names.
            String fileName = urlCustomer + " " + urlContractId + ".pdf";
            fileName = fileName.Replace(" ", "_");
            ViewData["Title"] = fileName;
            Response.Headers["Content-disposition"] = "inline; filename=" + fileName;
                

            return pdf;
        }
        public FileStream updateFooter(String footerPath, view_User user)
        {
            String footerTxtPath = Server.MapPath("~/Views/CustomerOffer/Footer.txt").Replace("\\", "/");
            String content = System.IO.File.ReadAllText(footerTxtPath);
            FileStream fs = new FileStream(footerPath, FileMode.Create, FileAccess.ReadWrite);
            content += @"<body onload='subst()'>
                            <div class='container'>
	                            <div class='page-numbers'>
     	                            Sida <span class='page'></span> av <span class='topage'></span>
                                    </div>";
            if (user.Use_logo)
                content += @"<div class='footer-logo'>
                            <img src='../../Content/img/tieto-logo.png' alt='tieto-logo' />
                        </div>";
            content += @"</div>
                    </body>
                    </html>
                    ";
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(content);

            writer.Close();
            return fs;
        }




        public FileStream updateHeader(String headerPath, view_User user)
        {
            String headerTxtPath = Server.MapPath("~/Views/CustomerOffer/Header.txt").Replace("\\", "/");
            String content = System.IO.File.ReadAllText(headerTxtPath);
            FileStream fs = new FileStream(headerPath, FileMode.Create, FileAccess.Write);
            content += @"<body>
                        <div class='header'>
                            <div id='date' class='date'>" + DateTime.Now.ToString("yyy-MM-dd") + "</div>";
            if (user.Use_logo)
            {
                content += @"<div class='logo'>
                            <img src='../../Content/img/tieto-logo-com.png' />
                            </div> ";
            }
            content += @"</div>
                        </body>
                        </html>
                    ";
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(String.Empty);
            writer.Write(content);

            writer.Close();
            return fs;
        }
        public ActionResult Header()
                    {
            return View("Header");
        }

        public String GenerateStaticHeader(/*String ssmaTimestamp*/)
        {
            String userName = WindowsIdentity.GetCurrent().Name.Replace(@"\", "_");
            String fileName = "Header_" + userName + ".html";
            String filePath = Server.MapPath("~/Views/CustomerContract/Headers/" + fileName);

            FileInfo info = new FileInfo(filePath);
            
            using (StreamWriter writer = info.CreateText())
            {
                writer.WriteLine(RenderActionResultToString(Header()));
            }
            
            return filePath;
        }

        private string RenderActionResultToString(ActionResult result)
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            // Create memory writer.
            var sb = new StringBuilder();
            var memWriter = new StringWriter(sb);

            // Create fake http context to render the view.
            var fakeResponse = new HttpResponse(memWriter);
            var fakeContext = new HttpContext(System.Web.HttpContext.Current.Request,
                fakeResponse);
            var fakeControllerContext = new ControllerContext(
                new HttpContextWrapper(fakeContext),
                this.ControllerContext.RouteData,
                this.ControllerContext.Controller);
            var oldContext = System.Web.HttpContext.Current;
            System.Web.HttpContext.Current = fakeContext;

            // Render the view.
            result.ExecuteResult(fakeControllerContext);

            // Restore old context.
            System.Web.HttpContext.Current = oldContext;

            // Flush memory and return output.
            memWriter.Flush();
            return sb.ToString();
        }

        public String CustomerContractJsonData()
        {
            String customer = Request.Form["customer"];
            List<String> customerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            if (customer == "" || customer == null)
            {
                if(customerNames.Count <= 0)
                    customer = "";
                else
                    customer = customerNames[0];
            }

            List<view_Contract> customerContracts = view_Contract.GetContracts(customer);
            List<Dictionary<String, dynamic>> contracts = new List<Dictionary<String, dynamic>>();

            foreach (view_Contract contract in customerContracts)
            {
                /*view_CustomerContact cc = new view_CustomerContact();
                cc.Select("Customer = '" + contract.Customer + "' AND Contact_person = '" + contract.Contact_person + "'");*/

                Dictionary<String, dynamic> variables = new Dictionary<String, dynamic>();
                foreach (System.Reflection.PropertyInfo pi in contract.GetType().GetProperties())
                {
                    if(!pi.Name.StartsWith("_"))
                    {
                        if (pi.Name == "Extension")
                            variables.Add(pi.Name, contract.getStringExtension());
                        else if (pi.Name == "Term_of_notice")
                            variables.Add(pi.Name, contract.getStringTON());
                        else if (pi.Name == "Customer")
                            continue;
                        else
                            variables.Add(pi.Name, pi.GetValue(contract));
                    }
                        
                }

                contracts.Add(variables);
            }

            this.Response.ContentType = "text/plain";
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(contracts) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }

        public String SaveContractHead()
        {
            try
            {
                String customerString = Request.Unvalidated.Form["customer"];
                String contractId = Request.Form["contract-id"];
                String json = Request.Unvalidated.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_ContractHead contractHead = new view_ContractHead();
                contractHead.Select("Customer = '" + customerString + "' AND Contract_id = '" + contractId + "'");
                if (contractHead.Contract_id == null)
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contractHead.SetValue(variable.Key, System.Web.HttpUtility.HtmlDecode(variable.Value.ToString()));
                    }
                    view_Customer customer = new view_Customer();
                    customer.Select("Customer = '" + customer + "'");
                    contractHead.Address = customer.Address;
                    contractHead.City = customer.City;
                    contractHead.Buyer = customer.Customer;
                    contractHead.Zip_code = customer.Zip_code;
                    contractHead.Corporate_identity_number = customer.Corporate_identity_number;


                    contractHead.Insert();
                }
                else
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contractHead.SetValue(variable.Key, System.Web.HttpUtility.HtmlDecode(variable.Value.ToString()));
                    }
                    contractHead.Update("Customer = '" + customerString + "' AND Contract_id = '" + contractId + "'");
                }


                view_Contract cont = new view_Contract("Customer = '" + customerString + "' AND Contract_id = '" + contractId + "'");
                cont.Updated = System.DateTime.Now;
                cont.Contact_person = contractHead.Contact_person;
                cont.Update("Customer = '" + customerString + "' AND Contract_id = '" + contractId + "'");



                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String Insert()
        {
            try
            {
                String json = Request.Form["json"];
                view_Contract a = null;
                try
                {
                    a = (view_Contract)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Contract));
                    int i = 1;
                    bool foundIndex = false;
                    String contractId = "";
                    view_Contract contract = new view_Contract();
                    while(!foundIndex) // make sure that we will get a unique contract id
                    {
                        if (a.Is(ContractType.ModuleTermination))
                        {
                            contractId = "XIT " + System.Web.HttpContext.Current.GetUser().Sign + " " + DateTime.Now.ToShortDateString() + " " + i.ToString("00");
                        }
                        else
                        {
                            contractId = System.Web.HttpContext.Current.GetUser().Default_system + " " + System.Web.HttpContext.Current.GetUser().Sign + " " + DateTime.Now.ToShortDateString() + " " + i.ToString("00");
                        }
                        contract = new view_Contract();
                        contract.Select("Contract_id = '" + contractId + "'");
                        if(contract.Contract_id == null)
                            foundIndex = true;
                        i++;
                    }

                    a.Contract_id = contractId;
                    a.Created = System.DateTime.Now;
                    a.Updated = System.DateTime.Now;
                    if (a.Is(ContractType.MainContract))
                    {
                        a.Main_contract_id = contractId;
                    }
                }
                catch (Exception e)
                {
                    return "0";
                }

                view_ContractHead contractHead = new view_ContractHead();
                view_Customer customer = new view_Customer();
                customer.Select("Customer = '" + a.Customer + "'");
                contractHead.Contract_id = a.Contract_id;
                contractHead.Address = customer.Address;
                contractHead.City = customer.City;
                contractHead.Buyer = customer.Customer;
                contractHead.Customer = customer.Customer;
                contractHead.Zip_code = customer.Zip_code;
                contractHead.Corporate_identity_number = customer.Corporate_identity_number;
                contractHead.Contact_person = a.Contact_person;
                contractHead.Customer_sign = a.Contact_person.Substring(0, Math.Min(a.Contact_person.Length, 50));
                contractHead.Our_sign = System.Web.HttpContext.Current.GetUser().Name.Substring(0, Math.Min(System.Web.HttpContext.Current.GetUser().Name.Length, 50));
                
                contractHead.Insert();

                a.Insert();

                return a.Contract_id.ToString();
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String GetContacts()
        {
            String customer = Request.Form["customer"];
            List<view_CustomerContact> l = view_CustomerContact.getAllCustomerContacts(customer);
            foreach (view_CustomerContact contact in l)
            {
                contact.Customer = System.Web.HttpUtility.HtmlEncode(contact.Customer);
                contact.Contact_person = System.Web.HttpUtility.HtmlEncode(contact.Contact_person);
                contact.Email = System.Web.HttpUtility.HtmlEncode(contact.Email);
            }

            return (new JavaScriptSerializer()).Serialize(l);
        }

        public String SaveContact()
        {

            try
            {
                String json = Request.Unvalidated.Form["json"];
                String id = Request.Form["id"];
                Dictionary<String, dynamic> map = null;
                try
                {
                    map = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                view_CustomerContact a = new view_CustomerContact();

                String compareValue1 = System.Web.HttpUtility.HtmlDecode(Convert.ToString(map["Customer"]));
                String compareValue2 = System.Web.HttpUtility.HtmlDecode(Convert.ToString(map["oldName"]));
                String compareValue3 = System.Web.HttpUtility.HtmlDecode(Convert.ToString(map["oldEmail"]));
                a.Select("Customer = '" + compareValue1 + "' AND Contact_person = '" + compareValue2 + "' AND Email = '" + compareValue3 + "'");
                foreach (KeyValuePair<String, object> entry in map)
                {
                    if (entry.Key != "oldName" && entry.Key != "oldEmail")
                        a.SetValue(entry.Key, System.Web.HttpUtility.HtmlDecode(entry.Value.ToString()));
                }

                a.Update("Customer = '" + compareValue1 + "' AND Contact_person = '" + compareValue2 + "' AND Email = '" + compareValue3 + "'");

                foreach (view_CustomerOffer co in view_CustomerOffer.getAllCustomerOffers(compareValue1))
                {
                    if (co.Contact_person == compareValue2)
                    {
                        co.Contact_person = a.Contact_person;
                        co.Update("Offer_number = " + co._Offer_number);
                    }
                }

                foreach(view_Contract contract in view_Contract.GetContracts(compareValue1))
                {
                    if(contract.Contact_person == compareValue2)
                    {
                        contract.Contact_person = a.Contact_person;
                        contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                    }
                }

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String SaveContractFromOffer()
        {
            try
            {
                //String ssma_timestamp = Request.Form["ssma_timestamp"];
                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
                List<dynamic> modules = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(Request.Form["modules"], typeof(List<dynamic>));
                List<Dictionary<String, dynamic>> services = (List<Dictionary<String, dynamic>>)(new JavaScriptSerializer()).Deserialize(Request.Form["services"], typeof(List<Dictionary<String, dynamic>>));

                foreach(dynamic article in modules)
                {
                    view_Module module = new view_Module();
                    module.Select("Article_number =  " + article["Article_number"]);

                    view_ContractRow cRow = new view_ContractRow();
                    cRow.Article_number = Convert.ToInt32(module.Article_number);
                    cRow.Customer = contract.Customer;
                    cRow.Contract_id = contract.Contract_id;
                    cRow.Created = DateTime.Now;
                    cRow.License = Convert.ToDecimal(article["License"]);
                    cRow.Maintenance = Convert.ToDecimal(article["Maintenance"]);
                    cRow.New = true;
                    cRow.Rewritten = false;
                    cRow.Removed = false;

                    cRow.Delete("Article_number = " + cRow.Article_number + " AND Contract_id = '" + cRow.Contract_id + "' AND Customer = '" + cRow.Customer + "'");
                    cRow.Insert();
                }

                foreach(dynamic serviceObj in services)
                {
                    view_Service service = new view_Service();
                    service.Select("Code =  " + serviceObj["code"]);

                    view_ContractConsultantRow ccRow = new view_ContractConsultantRow();

                    ccRow.Code = service.Code;
                    ccRow.Customer = contract.Customer;
                    ccRow.Contract_id = contract.Contract_id;
                    ccRow.Created = DateTime.Now;
                    ccRow.Amount = serviceObj["amount"];
                    ccRow.Total_price = serviceObj["total_price"];

                    ccRow.Delete("Code = " + ccRow.Code + " AND Contract_id = '" + ccRow.Contract_id + "' AND Customer = '" + ccRow.Customer + "'");
                    ccRow.Insert();
                }
                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String SaveOptionFromActive()
        {
            try
            {
                //String ssma_timestamp = Request.Form["ssma_timestamp"];
                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
                List<String> options = (List<String>)(new JavaScriptSerializer()).Deserialize(Request.Form["options"], typeof(List<String>));

                foreach (String articleNumber in options)
                {
                    view_Module module = new view_Module();
                    module.Select("Article_number =  " + articleNumber);

                    List<Dictionary<String, object>> modulePrices = view_Module.getModuleWithCorrectPrice(module.System, contract.Customer, module.Classification);

                    Dictionary<String, object> modulePrice = modulePrices.First(m => module.Article_number == Convert.ToSingle(m["Article_number"]));

                    view_ContractRow cRow = new view_ContractRow();
                    cRow.Article_number = Convert.ToInt32(module.Article_number);
                    cRow.Customer = contract.Customer;
                    cRow.Contract_id = contract.Contract_id;
                    cRow.Created = DateTime.Now;
                    cRow.License = Convert.ToDecimal(modulePrice["License"]);
                    cRow.Maintenance = Convert.ToDecimal(modulePrice["Maintenance"]);
                    cRow.New = false;
                    cRow.Rewritten = false;
                    cRow.Removed = false;

                    cRow.Delete("Article_number = " + cRow.Article_number + " AND Contract_id = '" + cRow.Contract_id + "' AND Customer = '" + cRow.Customer + "'");
                    cRow.Insert();
                }
                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String SaveItemsFromContracts()
        {
            try
            {
                //String ssma_timestamp = Request.Form["ssma_timestamp"];
                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
                List<dynamic> modules = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(Request.Form["modules"], typeof(List<dynamic>));
                List<dynamic> services = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(Request.Form["services"], typeof(List<dynamic>));

                foreach (dynamic article in modules)
                {
                    view_Module module = new view_Module();
                    module.Select("Article_number =  " + article["Article_number"]);

                    List<Dictionary<String, object>> modulePrices = view_Module.getModuleWithCorrectPrice(module.System, contract.Customer, module.Classification);

                    Dictionary<String, object> modulePrice = modulePrices.First(m => module.Article_number == Convert.ToSingle(m["Article_number"]));

                    view_ContractRow cRow = new view_ContractRow();
                    cRow.Article_number = Convert.ToInt32(module.Article_number);
                    cRow.Customer = contract.Customer;
                    cRow.Contract_id = contract.Contract_id;
                    cRow.Created = DateTime.Now;
                    cRow.License = Convert.ToDecimal(article["License"]);
                    cRow.Maintenance = Convert.ToDecimal(article["Maintenance"]);
                    cRow.New = false;
                    cRow.Rewritten = true;
                    cRow.Removed = false;

                    cRow.Delete("Article_number = " + cRow.Article_number + " AND Contract_id = '" + cRow.Contract_id + "' AND Customer = '" + cRow.Customer + "'");
                    cRow.Insert();
                }
                foreach (dynamic serviceObj in services)
                {
                    view_Service service = new view_Service();
                    service.Select("Code =  " + serviceObj["code"]);

                    view_ContractConsultantRow ccRow = new view_ContractConsultantRow();

                    ccRow.Code = service.Code;
                    ccRow.Customer = contract.Customer;
                    ccRow.Contract_id = contract.Contract_id;
                    ccRow.Created = DateTime.Now;
                    ccRow.Amount = serviceObj["amount"];
                    ccRow.Total_price = decimal.Parse(serviceObj["total_price"]);

                    ccRow.Delete("Code = " + ccRow.Code + " AND Contract_id = '" + ccRow.Contract_id + "' AND Customer = '" + ccRow.Customer + "'");
                    ccRow.Insert();
                }

                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String GetSelectedModules()
        {
            //String ssma_timestamp = Request.Form["ssma_timestamp"];
            String urlCustomer = Request.Form["customer"];
            String urlContractId = Request.Form["contract-id"];
            view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");

            List<dynamic> modules = new List<dynamic>();
            foreach (view_ContractRow cRow in contract._ContractRows)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + cRow.Article_number);
                if (cRow.Alias != null && cRow.Alias != "")
                    module.Module = cRow.Alias;

                var obj = new
                {
                    Article = module.Module,
                    Article_number = module.Article_number,
                    Maintenance = cRow.Maintenance,
                    License = cRow.License,
                    Price_category = module.Price_category,
                    System = module.System,
                    Rewritten = cRow.Rewritten,
                    Removed = cRow.Removed,
                    NewMod = cRow.New,
                };

                modules.Add(obj);
            }

            return (new JavaScriptSerializer()).Serialize(modules);
        }

        public String GetSelectedOptions()
        {
           
            String urlCustomer = Request.Form["customer"];
            String urlContractId = Request.Form["contract-id"];
           
            view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");

            List<dynamic> modules = new List<dynamic>();
            foreach (view_ContractOption cOption in contract._ContractOptions)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + cOption.Article_number);

                var obj = new
                {
                    Article = module.Module,
                    Article_number = module.Article_number,
                    Maintenance = cOption.Maintenance,
                    License = cOption.License,
                    System = module.System
                };

                modules.Add(obj);
            }
           
            return (new JavaScriptSerializer()).Serialize(modules);

        }

        public List<dynamic> GetSelectedOptionsList(String customer, String contractId)
        {
            
            view_Contract contract = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");

            List<dynamic> modules = new List<dynamic>();
            foreach (view_ContractOption cOption in contract._ContractOptions)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + cOption.Article_number);
                dynamic obj = new ExpandoObject();
                
                obj.Article = module.Module;
                obj.Article_number = module.Article_number;
                obj.Maintenance = cOption.Maintenance;
                obj.License = cOption.License;
                obj.System = module.System;
                obj.Classification = module.Classification;
          

                modules.Add(obj);
            }
            
            return modules;
        }

        public String GetSelectedServices()
        {
            //String ssma_timestamp = Request.Form["ssma_timestamp"];
            String urlCustomer = Request.Form["customer"];
            String urlContractId = Request.Form["contract-id"];
            view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");

            List<dynamic> services = new List<dynamic>();
            foreach (view_ContractConsultantRow ccRow in contract._ContractConsultantRows)
            {
                view_Service service = new view_Service();
                service.Select("Code = " + ccRow.Code);

                var obj = new
                {
                    Code = service.Code,
                    Description = service.Description,
                    Price = ccRow.Total_price / ccRow.Amount,
                    Amount = ccRow.Amount,
                    Total = ccRow.Total_price
                };

                services.Add(obj);
            }

            return (new JavaScriptSerializer()).Serialize(services);
        }

        private List<String> GetStatuses()
        {
            List<String> contractStatus = new List<string>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Status FROM dbo.Avtalsstatus";

                command.Prepare();



                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        contractStatus.Add(reader["Status"].ToString());
                    }
                }


            }
            return contractStatus;
        }

        private List<String> GetContractTypes()
        {
            List<String> contractStatus = new List<string>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Avtalstyp FROM dbo.Avtalstyp";

                command.Prepare();



                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        contractStatus.Add(reader["Avtalstyp"].ToString());
                    }
                }


            }
            return contractStatus;
        }

        public String GetContractHead()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];

                view_ContractHead contractHead = new view_ContractHead();
                contractHead.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                return (new JavaScriptSerializer()).Serialize(contractHead);
            }
            catch
            {
                return "0";
            }
            
        }

        public String GetContactPersons()
        {
            try
            {
                String customer = Request.Form["customer"];
                //String contractId = Request.Form["contract-id"];

                List<String> contactPersons = view_CustomerContact.getAllCustomerContacts(customer).Select(c => c.Contact_person).ToList();

                for (int i = 0; i < contactPersons.Count; i++)
                {
                    contactPersons[i] = System.Web.HttpUtility.HtmlEncode(contactPersons[i]);
                }

                return (new JavaScriptSerializer()).Serialize(contactPersons);
            }
            catch
            {
                return "0";
            }
        }

        public String UpdateContractRows()
        {
            try
            {
                String selectedArticles = Request.Form["Object"];
                //int ssma_timestamp = Convert.ToInt32(Request.Form["ssma_timestamp"]);

                List<dynamic> list = null;
                try
                {
                    list = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(selectedArticles, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                return "0";
            }

                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                String urlctrResign = Request.Form["ctrResign"];

                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
                // remove all consultant row to later insert the new ones
                //List<view_ContractRow> rewrittens = new List<view_ContractRow>();
                foreach (view_ContractRow cr in contract._ContractRows)
                {
                    //if (cr.Rewritten == true)
                    //    rewrittens.Add(cr);
                    cr.Delete("Customer = '" + cr.Customer + "' AND Contract_id = '" + cr.Contract_id + "' AND Article_number = " + cr.Article_number);
                }

                foreach (Dictionary<String, Object> dict in list)
                {
                    int Article_number = Convert.ToInt32(dict["Article_number"]);
                    double License = 0;
                    if (dict.Keys.Contains("License"))
                        License = double.Parse(dict["License"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
                    double Maintenance = double.Parse(dict["Maintenance"].ToString().Replace(",", "."), CultureInfo.InvariantCulture);
                    int RowType = Convert.ToInt32(dict["Rowtype"]);


                    view_ContractRow offerRow = new view_ContractRow();
                    offerRow.Customer = contract.Customer;
                    offerRow.Contract_id = contract.Contract_id;
                    offerRow.Article_number = Article_number;
                    offerRow.License = Convert.ToDecimal(License);
                    offerRow.Maintenance = Convert.ToDecimal(Maintenance);
                    offerRow.Alias = dict["Alias"].ToString();
                    if (RowType == 3)
                    {
                        offerRow.New = false;
                        if (urlctrResign == "True")
                        {
                            offerRow.New = true;
                        }
                        offerRow.Rewritten = false;
                        offerRow.Removed = false;
                    }
                    if (RowType == 2)
                    {
                        offerRow.New = false;
                        offerRow.Rewritten = true;
                        offerRow.Removed = true;
                    }
                    if (RowType == 1)
                    {
                        offerRow.New = false;
                        offerRow.Rewritten = true;
                        offerRow.Removed = false;
                    }
                    //if(rewrittens.Any(m => m.Article_number == Article_number))
                    //{
                    //    offerRow.New = false;
                    //    offerRow.Rewritten = true;
                    //}
                    //else
                    //{
                    //    offerRow.New = true;
                    //    offerRow.Rewritten = false;
                    //}
                    offerRow.Insert();
                }
                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                return "1";
            }
            catch{
                return "-1";
            }
        }

        public String UpdateContractOptions()
        {
            try
            {
                String selectedArticles = Request.Form["Object"];
                //int ssma_timestamp = Convert.ToInt32(Request.Form["ssma_timestamp"]);

                List<dynamic> list = null;
                try
                {
                    list = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(selectedArticles, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
                // remove all consultant row to later insert the new ones
                foreach (view_ContractOption co in contract._ContractOptions)
                {
                    co.Delete("Customer = '" + co.Customer + "' AND Contract_id = '" + co.Contract_id + "' AND Article_number = " + co.Article_number);
                }

                foreach (Dictionary<String, Object> dict in list)
                {
                    int Article_number = Convert.ToInt32(dict["Article_number"]);
                    double License = 0;
                    if (dict.Keys.Contains("License"))
                        License = double.Parse(dict["License"].ToString(), CultureInfo.InvariantCulture);
                    double Maintenance = double.Parse(dict["Maintenance"].ToString(), CultureInfo.InvariantCulture);

                    view_ContractOption option = new view_ContractOption();
                    option.Customer = contract.Customer;
                    option.Contract_id = contract.Contract_id;
                    option.Article_number = Article_number;
                    option.License = Convert.ToDecimal(License);
                    option.Maintenance = Convert.ToDecimal(Maintenance);
                    option.Date = DateTime.Now;
                    option.Choice = false;
                    option.Insert();
                }
                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String UpdateContractConsultantRows()
        {
            String obj = Request.Form["object"];
            //int ssma_timestamp = Convert.ToInt32(Request.Form["ssma_timestamp"]);

            List<dynamic> list = null;
                try
                {
                list = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(obj, typeof(List<dynamic>));
                }
            catch (Exception e)
                {
                    return "0";
                }

                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");
            // remove all consultant row to later insert the new ones
            foreach (view_ContractConsultantRow cr in contract._ContractConsultantRows)
            {
                cr.Delete("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "' AND Code = " + cr.Code);
            }

            foreach (Dictionary<String, Object> dict in list)
            {
                int id = Convert.ToInt32(dict["id"]);
                int amount = Convert.ToInt32(dict["amount"]);
                int total = Convert.ToInt32(dict["total"]);

                view_ContractConsultantRow consultantRow = new view_ContractConsultantRow();
                consultantRow.Contract_id = contract.Contract_id;
                consultantRow.Customer = contract.Customer;
                consultantRow.Code = id;
                consultantRow.Amount = amount;
                consultantRow.Total_price = total;
                consultantRow.Created = DateTime.Now;
                consultantRow.Insert();
            }
           
            contract.Updated = System.DateTime.Now;
            contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");

            return "1";
        }

        public String GetModules()
        {
            String customer = Request.Form["customer"];
            String System = Request.Form["System"];
            String classification = Request.Form["classification"];
            String ctr = Request.Form["contracttype"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryText = "";

                if (ctr == "M")
                {
                    queryText = @"SELECT * FROM qry_GetModulesContractTermination 
                                    WHERE System = @System AND Classification = @classification And Customer = @customer
                                    order by Article_number asc";
                }
                else
                {
                    queryText = @"SELECT * FROM qry_GetModulesContractNormal 
                                    WHERE System = @System AND Classification = @classification And Customer = @customer
                                    order by Article_number asc";
                }
                       
//                String queryText = @"SELECT view_Module.Article_number, view_Module.Module, view_Tariff.License, view_Tariff.Maintenance, 
//                                    view_Module.Price_category, view_Module.System, view_Module.Classification,view_Module.Comment
//                                    FROM view_Module                                                                                       
//                                    JOIN view_Tariff                                                                                       
//                                    on view_Module.Price_category = view_Tariff.Price_category
//                                    WHERE System = @System AND Classification = @classification And Expired = 0
//                                    AND Inhabitant_level = (
//                                        Select ISNULL(Inhabitant_level, 1) AS I_level from view_Customer
//                                        where Customer = @customer
//                                    )
//                                    order by Article_number asc";

                // Default query
                command.CommandText = queryText;

                command.Prepare();
                command.Parameters.AddWithValue("@System", System);
                command.Parameters.AddWithValue("@classification", classification);
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();
                List<IDictionary<String, object>> resultList = new List<IDictionary<String, object>>();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, object> result = new Dictionary<String, object>();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {

                                result.Add(reader.GetName(i), reader.GetValue(i));



                                i++;
                            }
                            result["License"] = result["License"].ToString().Replace(",", ".");
                            result["Maintenance"] = result["Maintenance"].ToString().Replace(",", ".");
                            result["Price_category"] = result["Price_category"].ToString().Replace(",", ".");
                            result["System"] = result["System"].ToString();
                            resultList.Add(result);
                        }
                    }
                }
                Response.ContentType = "text/plain";

                List<view_ContractRow> rows = view_ContractRow.GetValidContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();

                foreach (Dictionary<String, dynamic> kv in resultList)
                {
                    if (rows.Any(cr => cr.Article_number == kv["Article_number"] && cr.Removed == false) && ctr != "M")
                        kv.Add("Used", true);
                }

                //Response.Charset = "UTF-8";
                // this.solve();
                String resultString = (new JavaScriptSerializer()).Serialize(resultList);
                return resultString;
            }
        }

        public String GetModulesAll(){

            String customer = Request.Form["customer"];
            String searchtext = Request.Form["searchtext"];
            String ctr = Request.Form["contracttype"];
            String contractid = Request.Form["contractid"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryText = "";

                if (ctr == "M")
                {
                    queryText = @"SELECT * FROM qry_GetModulesContractTermination
                                    WHERE Customer = @customer And (Cast(Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(Article_number As Varchar(30)) Else @searchtext End Or
                                    Module Like Case @searchtext When '' Then Module Else @searchtext End) 
                                    order by Article_number asc";
                }
                else
                {
                    queryText = @"SELECT * FROM qry_GetModulesContractNormal 
                                    WHERE Customer = @customer And (Cast(Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(Article_number As Varchar(30)) Else @searchtext End Or
                                    Module Like Case @searchtext When '' Then Module Else @searchtext End) 
                                    order by Article_number asc";
                }

//                String queryText = @"SELECT view_Module.Article_number, view_Module.Module, view_Tariff.License, view_Tariff.Maintenance,
//                                    view_Module.Price_category, view_Module.System, view_Module.Classification, view_Module.Comment
//                                    FROM view_Module                                                                                       
//                                    JOIN view_Tariff                                                                                       
//                                    on view_Module.Price_category = view_Tariff.Price_category
//                                    WHERE Expired = 0 And (Cast(view_Module.Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(view_Module.Article_number As Varchar(30)) Else @searchtext End Or
//                                    view_Module.Module Like Case @searchtext When '' Then view_Module.Module Else @searchtext End)
//                                    AND Inhabitant_level = (
//                                        Select ISNULL(Inhabitant_level, 1) AS I_level from view_Customer
//                                        where Customer = @customer
//                                    )
//                                    order by Article_number asc";

                // Default query
                command.CommandText = queryText;

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);
                command.Parameters.AddWithValue("@searchtext", "%" + searchtext + "%");

                command.ExecuteNonQuery();
                List<IDictionary<String, object>> resultList = new List<IDictionary<String, object>>();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, object> result = new Dictionary<String, object>();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {

                                result.Add(reader.GetName(i), reader.GetValue(i));



                                i++;
                            }
                            result["License"] = result["License"].ToString().Replace(",", ".");
                            result["Maintenance"] = result["Maintenance"].ToString().Replace(",", ".");
                            result["Price_category"] = result["Price_category"].ToString().Replace(",", ".");
                            result["System"] = result["System"].ToString();
                            resultList.Add(result);
                        }
                    }
                }
                Response.ContentType = "text/plain";

                List<view_ContractRow> rows = view_ContractRow.GetAllContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();

                foreach (Dictionary<String, dynamic> kv in resultList)
                {
                    if (rows.Any(cr => cr.Article_number == kv["Article_number"] && cr.Removed == false) && ctr != "M")
                        kv.Add("Used", true);

                    //if (rows.Any(cr => cr.Article_number == kv["Article_number"] && cr.Contract_id == contractid && cr.Rewritten == true) && ctr != "M")
                    //    kv.Add("Rewritten", true);

                    //if (rows.Any(cr => cr.Article_number == kv["Article_number"] && cr.Contract_id == contractid && cr.New == true) && ctr != "M")
                    //    kv.Add("New", true);

                    //if (rows.Any(cr => cr.Article_number == kv["Article_number"] && cr.Contract_id == contractid && cr.Removed == true) && ctr != "M")
                    //    kv.Add("Removed", true);
                }
                //Response.Charset = "UTF-8";
                // this.solve();
                String resultString = (new JavaScriptSerializer()).Serialize(resultList);
                return resultString;

            }
        }


        public List<String> GetAllSystemNames()
        {
            List<String> SystemList = new List<String>();
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryTextClassification = @"SELECT DISTINCT Procapita FROM V_Procapita";
                command.CommandText = queryTextClassification;
                command.Prepare();

                command.ExecuteNonQuery();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SystemList.Add(reader["Procapita"].ToString());
                    }
                }
                    }

            return SystemList;
                }

        public List<String> GetMainContractsToResign(String customer)
        {
            List<String> MainCtrList = new List<String>();
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryTextClassification = @"Select contractid From qry_MainContractToResign Where Customer = @customer Order By 1";
                command.CommandText = queryTextClassification;
                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);

                command.ExecuteNonQuery();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            MainCtrList.Add(reader["contractid"].ToString());
                        }
                    }
                    else
                    {
                        MainCtrList.Add("--No Main contracts to resign--");
                    }
                }
            }

            return MainCtrList;
        }
        public String GetTemplates()
        {
            try
            {
                String sign = System.Web.HttpContext.Current.GetUser().Sign;
                //String timestamp = Request.Form["ssma_timestamp"];
                String urlCustomer = Request.Form["customer"];
                String urlContractId = Request.Form["contract-id"];
                view_Contract contract = new view_Contract("Customer = '" + urlCustomer + "' AND Contract_id = '" + urlContractId + "'");

                //List<KeyValuePair<String, int>> ids = view_TextTemplate.getAllTextTemplates("Alla").Where(t => t.Document_type == contract.Contract_type).Select(t => "Alla" + " - " + t.Short_descr + " (#" + t.ID_PK + "#)").ToList();

                List<KeyValuePair<String, int>> ids = view_TextTemplate.getTextTemplatesType("Avtal", sign).Where
                    (t => t.Document_type.Substring(1,2) == contract.Contract_type.Substring(1,2)).Select(t => new KeyValuePair<string, int>(t.Sign + " - " + t.Short_descr, t.ID_PK)).ToList();

                //ids.AddRange(view_TextTemplate.getTextTemplatesType("Avtal",sign).Where
                //    (t => t.Document_type == contract.Contract_type).Select(t => new KeyValuePair<string, int>("Alla" + " - " + t.Short_descr, t.ID_PK)).ToList());

                return (new JavaScriptSerializer()).Serialize(ids);
            }
            catch
            {
                return "0";
            }

                
        }

        public String GetTemplateText()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];
                String selected = Request.Form["selected"];
                if (selected.StartsWith("c", StringComparison.OrdinalIgnoreCase))
                {
                    view_ContractText text = new view_ContractText();
                    text.Select("Contract_id = '" + contractId + "' AND Customer = '" + customer + "'");
                    return (new JavaScriptSerializer()).Serialize(text);
                }
                else
                {
                    //char[] delim = {'#'};
                    ////char[] delim = { ' ', '-', ' ' };
                    //String[] selItem = selected.Split(delim, StringSplitOptions.RemoveEmptyEntries);
                    view_TextTemplate temp = new view_TextTemplate();
                    temp.Select("ID_PK = " + selected);
                    return (new JavaScriptSerializer()).Serialize(temp);
                }
                return "1";
            }
            catch
            {
                return "0";
            }
           
        }

        [ValidateInput(false)]
        public String SaveContractText()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];
                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_ContractText contractText = new view_ContractText();
                contractText.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                if (contractText.Contract_id == null)
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contractText.SetValue(variable.Key, variable.Value);
                    }
                    contractText.Insert();
                }
                else
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contractText.SetValue(variable.Key, variable.Value);
                    }
                    contractText.Update("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                }

                view_Contract cont = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                cont.Updated = System.DateTime.Now;
                cont.Update("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");

                return "1";
            }
            catch
            {
                return "-1";
            }

        }

        public String GetMainContracts()
        {
            try
            {
                String customer = Request.Form["customer"];

                List<String> mainContracts = view_Contract.GetContracts(customer).Where(c=>c.Is(ContractType.MainContract) && c.Status == "Giltigt").Select(c => c.Contract_id).ToList();
                
                return (new JavaScriptSerializer()).Serialize(mainContracts);
            }
            catch
            {
                return "0";
            }
        }
        public String GetContract()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];
               
                String jsonData =  (new JavaScriptSerializer()).Serialize( new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'"));

                return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
                {
                    DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                    dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                    return dt.ToString("yyyy-MM-dd");
                });
            }
            catch
            {
                return "0";
            }
        }

        public String UpdateTableInfo()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];
                String newContractId = Request.Form["newContract-id"];
                String json = Request.Form["json"];
                String oldStatus = Request.Form["oldStatus"];

                Dictionary<String, dynamic> map = null;
                try
                {
                    map = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                view_Contract a = new view_Contract();

                a.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");

                
                foreach (KeyValuePair<String, object> entry in map)
                {
                    if (String.IsNullOrEmpty(entry.Value.ToString()))
                    {
                        //short? nullString = null;
                        //a.SetValue(entry.Key, nullString);
                        a.SetValue(entry.Key, null);
                    }
                    else
                    {
                        a.SetValue(entry.Key, entry.Value);
                    }


                }
                //If contract ID has been changed, and it's a mani ciontract, then we also change the Main Contract ID
                if (a.Is(ContractType.MainContract))
                {
                    a.Main_contract_id = a.Contract_id;
                }

                a.Updated = System.DateTime.Now;
                a.Update("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");

                if (a.Resigned_contract != null && a.Status == "Giltigt" && oldStatus != "Giltigt")
                {
                    String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

                    using (var connection = new SqlConnection(connectionString))
                    using (var command = new SqlCommand("stp_SetExpire", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        connection.Open();
                        command.Prepare();
                        command.Parameters.AddWithValue("@pavtalsid", a.Contract_id);
                        command.Parameters.AddWithValue("@pomskrivid", a.Resigned_contract);
                        command.Parameters.AddWithValue("@pKund", customer);
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    
                }

                //ViewBag.CustomerContract.Contract_id = newContractId;

                //view_Contract cont = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + a.Contract_id + "'");
                return "1"/* + cont.SSMA_timestamp*/;
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String GetProlog(String customerP = null, String contractIdP = null)
        {
            try
            {
                String customer = "";
                String contractId = "";
                String from = "";
                if(customerP == null && contractIdP == null)
                {
                     customer = Request.Form["customer"];
                     contractId = Request.Form["contract-id"];
                     from = Request.Form["from"];
                }
                else
                {
                     customer = customerP;
                     contractId = contractIdP;
                     from = "current";
                }
                if (from == "current")
                {
                    String text = "";
                    view_ContractTemplate template = new view_ContractTemplate();
                    template.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                    if (String.IsNullOrEmpty(template.Prolog))
                    {
                        text = PrologTemplateToText(template);
                    }
                    else
                    {
                        text = template.Prolog;
                    }
                    return text;
                }
                else
                {
                    return MainContractText.GetProlog();
                }
            }
            catch
            {
                return "0";
            }
        }

        private String PrologTemplateToText(view_ContractTemplate template)
        {
            if (String.IsNullOrEmpty(template.Text1))
                return "";
            return "<p>" + template.Text1.Replace("\r\n","<br>") + "</p>";
        }

        public String GetEpilog(String customerP = null, String contractIdP = null)
        {
            try
            {
                String customer = "";
                String contractId = "";
                String from = "";
                if (customerP == null && contractIdP == null)
                {
                    customer = Request.Form["customer"];
                    contractId = Request.Form["contract-id"];
                    from = Request.Form["from"];
                }
                else
                {
                    customer = customerP;
                    contractId = contractIdP;
                    from = "current";
                }
                if (from == "current")
                {
                    String text = "";
                    view_ContractTemplate template = new view_ContractTemplate();
                    template.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                    if (String.IsNullOrEmpty(template.Epilog))
                    {
                        text = EpilogTemplateToText(template);
                    }
                    else
                    {
                        text = template.Epilog;
                    }
                    
                    return text;
                }
                else
                {
                    return MainContractText.GetEpilog();
                }
               
            }
            catch
            {
                return "0";
            }
        }

        private String EpilogTemplateToText(view_ContractTemplate template)
        {
            String text = "";
            
            text += "<h4>" + template.Title5 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text5))
                text += "<p>" + template.Text5.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title6 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text6))
                text += "<p>" + template.Text6.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title7 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text7))
                text += "<p>" + template.Text7.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title8 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text8))
                text += "<p>" + template.Text8.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title9 + "</h3>";
            text += "<h4>" + template.Title10 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text9))
                text += "<p>" + template.Text9.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title11 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text10))
                text += "<p>" + template.Text10.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title12 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text11))
                text += "<p>" + template.Text11.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title13 + "</h3>";
            text += "<h4>" + template.Title14 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text12))
                text += "<p>" + template.Text12.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title15 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text13))
                text += "<p>" + template.Text13.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title16 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text14))
                text += "<p>" + template.Text14.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title17 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text15))
                text += "<p>" + template.Text15.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title18 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text16))
                text += "<p>" + template.Text16.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title19 + "</h3>";
            text += "<h4>" + template.Title20 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text17))
                text += "<p>" + template.Text17.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title21 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text18))
                text += "<p>" + template.Text18.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title22 + "</h3>";
            text += "<h4>" + template.Title23 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text19))
                text += "<p>" + template.Text19.Replace("\r\n", "<br>") + "</p>";
            text += "<h4>" + template.Title24 + "</h4>";
            if (!String.IsNullOrEmpty(template.Text20))
                text += "<p>" + template.Text20.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title25 + "</h3>";
            if (!String.IsNullOrEmpty(template.Text21))
                text += "<p>" + template.Text21.Replace("\r\n", "<br>") + "</p>";
            text += "<h5>" + template.Title26 + "</h5>";
            text += "<p>" + template.Title27 + "</p>";
            if (!String.IsNullOrEmpty(template.Text22))
                text += "<p>" + template.Text22.Replace("\r\n", "<br>") + "</p>";
            text += "<h3>" + template.Title28 + "</h3>";
            if (!String.IsNullOrEmpty(template.Text23))
                text += "<p>" + template.Text23.Replace("\r\n", "<br>") + "</p>";
            return text;
        }

        [ValidateInput(false)]
        public String SaveMainContractText()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];
                String epilog = Request.Form["epilog"];
                String prolog = Request.Form["prolog"];
                String topHead = Request.Form["tophead"];
                String moduleText = Request.Form["moduleText"];
                view_ContractTemplate a = new view_ContractTemplate();
                
               // a.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                if (a.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'"))
                {
                    a.Epilog = epilog;
                    a.Prolog = prolog;
                    a.Title1 = topHead;
                    a.ModuleText = moduleText;
                    a.Update("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                }
                else
                {
                    a.Epilog = epilog;
                    a.Prolog = prolog;
                    a.Contract_id = contractId;
                    a.Customer = customer;
                    a.Title1 = topHead;
                    a.ModuleText = moduleText;
                    a.Insert();
                }

                view_Contract cont = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                cont.Updated = System.DateTime.Now;
                cont.Update("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");



                //view_Contract cont = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + a.Contract_id + "'");
                return "1"/* + cont.SSMA_timestamp*/;
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String GetTopHead(String customerP = null, String contractIdP = null)
        {
            try
            {
                String customer = "";
                String contractId = "";
                String from = "";
                if (customerP == null && contractIdP == null)
                {
                    customer = Request.Form["customer"];
                    contractId = Request.Form["contract-id"];
                    from = Request.Form["from"];
                }
                else
                {
                    customer = customerP;
                    contractId = contractIdP;
                    from = "current";
                }
                if (from == "current")
                {
                    
                    view_ContractTemplate template = new view_ContractTemplate();
                    template.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                    return template.Title1;
                }
                else
                {
                    return MainContractText.GetTitle1();
                }

            }
            catch
            {
                return "0";
            }
        }
        public String GetModuleText(String customerP = null, String contractIdP = null)
        {
            try
            {
                String customer = "";
                String contractId = "";
                String from = "";
                if (customerP == null && contractIdP == null)
                {
                    customer = Request.Form["customer"];
                    contractId = Request.Form["contract-id"];
                    from = Request.Form["from"];
                }
                else
                {
                    customer = customerP;
                    contractId = contractIdP;
                    from = "current";
                }
                if (from == "current")
                {
                    String text = "";
                    view_ContractTemplate template = new view_ContractTemplate();
                    template.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                    if (String.IsNullOrEmpty(template.ModuleText) && !String.IsNullOrEmpty(template.Text4))
                    {
                        text = ModuleTemplateToText(template);
                    }
                    else if (!String.IsNullOrEmpty(template.ModuleText))
                    {
                        text = template.ModuleText;
                    }
                    else
                    {
                        text = "";
                    }
                    return text;
                }
                else
                {
                    return MainContractText.GetModuleText();
                }
            }
            catch
            {
                return "0";
            }
        }

        private String ModuleTemplateToText(view_ContractTemplate template)
        {
            return "<p>" + template.Text4.Replace("\r\n", "<br>") + "</p>";
        }

        public String FinalizeModuleTermination()
        {
            string avtalsid = Request.Form["avtalsid"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("stp_FinalizeModuleTerminations", connection)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                connection.Open();
                command.Prepare();
                command.Parameters.AddWithValue("@pavtalsid", avtalsid);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return "1";
        }
        public String SaveResign()
        {
            string avtalsid = Request.Form["contractid"];
            string customer = Request.Form["customer"];
            string resignid = Request.Form["resignid"];
            string withtext = Request.Form["withtext"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("stp_ResignContract", connection)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                connection.Open();
                command.Prepare();
                command.Parameters.AddWithValue("@pavtalsid", avtalsid);
                command.Parameters.AddWithValue("@pkund", customer);
                command.Parameters.AddWithValue("@pomskrivid", resignid);
                command.Parameters.AddWithValue("@pwithtext", withtext);
                command.ExecuteNonQuery();
                connection.Close();
            }

            return "1";

        }
        public String ToggleRewritten()
        {
            string avtalsid = Request.Form["contractid"];
            string customer = Request.Form["customer"];
            string artnr = Request.Form["artnr"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("stp_ToggleRewritten", connection)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                connection.Open();
                command.Prepare();
                command.Parameters.AddWithValue("@pavtalsid", avtalsid);
                command.Parameters.AddWithValue("@pkund", customer);
                command.Parameters.AddWithValue("@partnr", artnr);
                command.ExecuteNonQuery();
                connection.Close();
            }

            return "1";

        }

        public String DeleteContract()
        {
            try
            {
                String value = Request.Form["id"];
                view_Contract co = new view_Contract("Contract_id='" + value + "'");
                //a.Select("Article_number = " + value);
                if (co.Status == "Makulerat")
                    co.Delete("Contract_id='" + value + "'");
                else
                    return "-1";
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }

        public string checkReminder()
        {
            String customer = Request.Form["customer"];

            view_Reminder vR = new view_Reminder();

            String remindExist = vR.checkIfReminderPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Default_system, System.Web.HttpContext.Current.GetUser().Sign);

            return remindExist;
        }

        public String GetReminders()
        {
            String customer = Request.Form["customer"];
            List<view_Reminder> vR = view_Reminder.getRemindersPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Default_system, System.Web.HttpContext.Current.GetUser().Sign);

            foreach (view_Reminder v in vR)
            {
                v.Customer_name = System.Web.HttpUtility.HtmlEncode(v.Customer_name);
            }

            String jsonData = (new JavaScriptSerializer()).Serialize(vR);
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
            //return (new JavaScriptSerializer()).Serialize(vR);

        }

        public string DeactivateReminder()
        {
            String id = Request.Form["id_pk"];
            view_Reminder a = new view_Reminder();

            a.deactivateReminder(Int32.Parse(id));

            return "1";

        }

    }
}