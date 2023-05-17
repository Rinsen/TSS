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
using System.Net;

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

        private List<String> skipProp = new List<string>
        {
            "Created",
            "Updated",
            "Option_date",
            "Resigned_contract",
            "Sign",
            "_ID",
            "Monthly_fee_from",
            "SSMA_timestamp"
        };

        //Request["selected-contract"]
        // GET: CustomerContract
        public ActionResult Index()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign).OrderBy(c => c).ToList());
            else
                this.ViewData.Add("Customers", view_Customer.getCustomerNames().OrderBy(c => c).ToList());
            ViewData.Add("Users", view_User.getAllUsers());
            this.ViewData.Add("Summera", System.Web.HttpContext.Current.GetUser().Std_sum_kontrakt);
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

            if (Request.QueryString["our_sign"] == null || Request.QueryString["our_sign"] == "")
            {
                ViewData.Add("CurrentUser", System.Web.HttpContext.Current.GetUser().Sign);
            }
            else
            {
                ViewData.Add("CurrentUser", Request.QueryString["our_sign"]);
            }

            ViewData.Add("showModalReminder", (System.Web.HttpContext.Current.GetUser().Reminder_Prompt == 1));

            if (Request.QueryString["customer"] != null)
            {
                ViewData.Add("SelectedCustomer", Request.QueryString["customer"]);
            }
            else
            {
                ViewData.Add("SelectedCustomer", "");
            }

            this.ViewData.Add("OfferNumber", on);

            this.ViewData.Add("Properties", typeof(view_Contract).GetProperties());
            this.ViewData.Add("SkipProperties", this.skipProp);
            this.ViewData.Add("Statuses", GetStatuses());
            this.ViewData.Add("ContractTypes", GetContractTypes());
            //this.ViewData.Add("ContractTypes", (new view_Contract()).GetSelectOptions("ContractType"));
            this.ViewData["Title"] = "Customer Contract";

            if (Request.QueryString["customer"] == null || Request.QueryString["customer"] == "")
            {
                List<view_Appointment> vA = view_Appointment.getAllAppointments(on).Where(a => (a.Date - DateTime.Now).TotalDays <= 30 && (a.Date - DateTime.Now).TotalDays >= 0).OrderBy(a => a.Date).ToList();
                if (vA.Count > 0)
                {
                    view_Appointment lastVisit = List_Management.AppointmentController.GetLastVisit(on);
                    vA.Add(lastVisit);
                }
                ViewData["Appointments"] = vA;
                ViewData.Add("Customer", on);
            }
            else
            {
                List<view_Appointment> vA = view_Appointment.getAllAppointments(Request["customer"]).Where(a => (a.Date - DateTime.Now).TotalDays <= 30 && (a.Date - DateTime.Now).TotalDays >= 0).OrderBy(a => a.Date).ToList();
                if (vA.Count > 0)
                {
                    view_Appointment lastVisit = List_Management.AppointmentController.GetLastVisit(Request["customer"]);
                    vA.Add(lastVisit);
                }
                ViewData["Appointments"] = vA;
                ViewData.Add("Customer", Request["customer"]);
            }
                
            
            return View();
        }

        /// <summary>
        /// Get article for article dependency logic in article dialog
        /// </summary>
        /// <returns></returns>
        public string GetModuleByArticleNumber()
        {
            string article_number = Request["article_number"];
            string customer = Request["customer"];
            string contract_id = Request["contract_id"];

            view_Module module = new view_Module();
            module.Select("Article_number = " + article_number);

            if(module.Module_type == 1) //Article
            {
                view_Customer contractCustomer = new view_Customer("Customer = " + customer);

                decimal? fixedPrice = null;
                view_Tariff tariff = new view_Tariff();

                if (module.Fixed_price.HasValue && module.Fixed_price.Value)
                {
                    fixedPrice = module.Price_category;
                }
                else
                {
                    tariff.Select("Inhabitant_level = " + contractCustomer.Inhabitant_level + " AND Price_category = " + ((int)module.Price_category.Value).ToString());
                }

                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract_id + " AND ModuleType = 'A' AND ModuleId = " + article_number);

                var returnObj = new
                {
                    Module = module.Module,
                    License = fixedPrice.HasValue ? fixedPrice.ToString() : tariff.License.HasValue && tariff.License.Value > 0 ? tariff.License.Value.ToString() : "0.00",
                    Maintenance = fixedPrice.HasValue ? "0.00" : tariff.Maintenance.HasValue && tariff.Maintenance.Value > 0 ? tariff.Maintenance.Value.ToString() : "0.00",
                    Module_status = module.Module_status,
                    Discount = module.Discount,
                    Discount_type = module.Discount_type,
                    Article_number = module.Article_number,
                    Contract_description = module.Contract_description,
                    Module_text_id = moduleText._ID
                };

                return (new JavaScriptSerializer()).Serialize(returnObj);
            }

            return null;
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
                else if (contractRequest == "_ModuleInfoSection")
                    return View(contractRequest);
                else
                    return View();
            }
            else
                return View();
        }


        public ActionResult ViewShippingListPdf()
        {
            this.GenerateThings();
            this.ViewData["Title"] = "Shipping List";


            ViewData["ArticleSystemDictionary"] = ((IList<KeyValuePair<String, List<dynamic>>>) ViewData["ArticleSystemDictionary"]).OrderBy(d => d.Key).ToList();

            ViewAsPdf pdf = new ViewAsPdf("ShippingListPdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + "Shipping List" + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"" + Request["contract-id"] +"\"";

            return pdf;
        }


        private void GenerateThings()
        {
           // string request = Request["selected-contract"];
            String urlCustomer = Request["customer"];
            String urlContractId = Request["contract-id"];
            String urlOurSign = Request["our_sign"];
            bool ctrResign = false;

            view_Contract contract = new view_Contract("Contract_id = '" + urlContractId + "' AND Customer = '" + urlCustomer + "'");
            if(contract._ContractConsultantRows != null)
            {
                contract._ContractConsultantRows = contract._ContractConsultantRows.OrderBy(o => o.Alias).ToList();
            }
            ViewData.Add("CustomerContract", contract);

            var moduleTexts = updateDescriptions(urlCustomer, urlContractId, false);
            var removedModuleTexts = updateDescriptions(urlCustomer, urlContractId, true);

            view_ContractText text = new view_ContractText();
            text.Select("Contract_id = '" + contract.Contract_id + "' AND Customer = '" + contract.Customer + "'");

            view_ContractText textRemoved = new view_ContractText();
            textRemoved.Module_info = removedModuleTexts;

            ViewData.Add("ContractText", text);
            ViewData.Add("ContractTextRemoved", textRemoved);

            ViewData.Add("Systems", GetAllSystemNames(contract.Area));
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

            foreach (view_Contract validContract in view_Contract.GetContracts(urlCustomer).Where(c => c.Status == "Giltigt"))
            {
                customersModules = new HashSet<view_ContractRow>(customersModules.Concat(validContract._ContractRows));
                customersServices = new HashSet<view_ContractConsultantRow>(customersServices.Concat(validContract._ContractConsultantRows));
            }

            // Already existing modules and services from former contracts
            ViewData.Add("CustomersModules", customersModules);
            ViewData.Add("CustomersServices", customersServices);
            // Already existing modules from former contracts except Removed modules
            //ViewData.Add("ActiveCustomerModules", new HashSet<view_ContractRow>(customersModules.Where(w => !w.Removed.HasValue || (w.Removed.HasValue && !w.Removed.Value))));

            List<dynamic> remArticles = new List<dynamic>();
            List<dynamic> remEducationPortals = new List<dynamic>();
            List<dynamic> oldArticles = new List<dynamic>();
            List<dynamic> oldEducationPortals = new List<dynamic>();
            List<dynamic> articles = new List<dynamic>();
            List<dynamic> educationPortals = new List<dynamic>();

            SortedList<String, List<dynamic>> articleSystemDic = new SortedList<String, List<dynamic>>();
            SortedList<String, List<dynamic>> articleAndServicesDic = new SortedList<String, List<dynamic>>();

            if(contract._ContractRows != null)
            foreach (view_ContractRow contractRow in contract._ContractRows)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + contractRow.Article_number);
                dynamic contractInfo = new ExpandoObject();
                contractInfo.Article_number = module.Article_number;
                contractInfo.Contract_id = contractRow.Contract_id;
                if(module.Read_name_from_module == 1)
                {
                    contractInfo.Module = module.Module;
                }
                else
                {
                    if (contractRow.Alias == null || contractRow.Alias == "")
                        contractInfo.Module = module.Module;
                    else
                        contractInfo.Module = contractRow.Alias;
                }
                contractInfo.System = module.System;
                contractInfo.Classification = module.Classification;
                contractInfo.License = contractRow.License;
                contractInfo.Maintenance = contractRow.Maintenance;
                contractInfo.Price_category = module.Price_category;
                contractInfo.Maint_price_category = module.Maint_price_category;
                contractInfo.Discount_type = module.Discount_type;
                contractInfo.Discount = module.Discount;
                contractInfo.IncludeDependencies = "false"; //We do this so that we don't add dependencies for "old" articles in selected-list. this also means that automatic removal does not work for "old" articles when opening article dialog

                view_Sector sector = new view_Sector();
                sector.Select("System=" + module.System + " AND Classification=" + module.Classification);

                contractInfo.Price_type = sector.Price_type;
                contractInfo.Fixed_price = contractRow.Fixed_price;
                contractInfo.Sort_number = sector.SortNo;
                contractInfo.Article_Sort_number = module.Sort_order;
                contractInfo.Expired = module.Expired;
                contractInfo.Id = contract._ID;
                contractInfo.Removed = contractRow.Removed;
                contractInfo.Rewritten = contractRow.Rewritten;
                contractInfo.RemovedFromContractId = contractRow.RemovedFromContractId;

                //New ModuleTexts
                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleType = 'A' AND ModuleId = " + contractRow.Article_number.ToString());
                contractInfo.ContractDescription = moduleText.Description == null ? module.Contract_description : moduleText.Description;
                contractInfo.ModuleTextId = moduleText._ID;
                contractInfo.ModuleType = "A";

                if (contractRow.Rewritten == true)
                    ctrResign = true;

                if (contractRow.Rewritten == true && contractRow.Removed == false)
                    oldArticles.Add(contractInfo);
                if (contractRow.Removed == true)
                    remArticles.Add(contractInfo);

                articles.Add(contractInfo);
                if (contractRow.Rewritten == false && contractRow.Removed == false) //Undviker att få med omskrivna och borttagna artiklar i NY-listan på kontraktet
                {
                    if (!articleSystemDic.ContainsKey(contractInfo.System))
                    {
                        articleSystemDic.Add(contractInfo.System, new List<dynamic> { contractInfo });
                    }
                    else
                    {
                        articleSystemDic[contractInfo.System].Add(contractInfo);
                    }

                    if (!articleAndServicesDic.ContainsKey(contractInfo.System))
                    {
                        articleAndServicesDic.Add(contractInfo.System, new List<dynamic> { contractInfo });
                    }
                    else
                    {
                        articleAndServicesDic[contractInfo.System].Add(contractInfo);
                    }
                }
            }

            if(contract._ContractConsultantRows != null)
            foreach (view_ContractConsultantRow consultantRow in contract._ContractConsultantRows)
            {
                view_Module service = new view_Module();
                service.Select("Article_number = " + consultantRow.Code.ToString());

                dynamic contractInfo = new ExpandoObject();

                contractInfo.Article_number = service.Article_number;
                if (service.Read_name_from_module == 1 || consultantRow.Alias == null || consultantRow.Alias == "")
                    contractInfo.Module = service.Module;
                else
                    contractInfo.Module = consultantRow.Alias;
                contractInfo.System = "Konsulttjänster";
                contractInfo.ModuleType = "K";                
                contractInfo.Price_category = service.Price_category;
                contractInfo.Article_Sort_number = service.Sort_order;
                contractInfo.Classification = service.Classification;

                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleType = 'K' AND ModuleId = " + consultantRow.Code.ToString());
                contractInfo.ContractDescription = moduleText.Description == null ? service.Contract_description : moduleText.Description;
                contractInfo.ModuleTextId = moduleText._ID;

                contractInfo.Id = contract._ID;

                contractInfo.Price_type = "Övrig";
                contractInfo.Fixed_price = service.Price_category;
                contractInfo.Sort_number = "20";

                if (!articleAndServicesDic.ContainsKey(contractInfo.System))
                {
                    articleAndServicesDic.Add(contractInfo.System, new List<dynamic> { contractInfo });
                }
                else
                {
                    articleAndServicesDic[contractInfo.System].Add(contractInfo);
                }
            }

            view_User usr = System.Web.HttpContext.Current.GetUser();

            //Här styrs sorteringen av artiklarna ut på avtalet gamla och borttagna artiklar
            if (usr.AvtalSortera == 1) //Article name
            {
                oldArticles = oldArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                oldEducationPortals = oldEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                remEducationPortals = remEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
            }
            else if (usr.AvtalSortera == 2) //Classification, Article name
            {
                oldArticles = oldArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                oldEducationPortals = oldEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                remEducationPortals = remEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
            }
            else if (usr.AvtalSortera == 3) //Classification, Article-no. Denna sortering styrs från view_ContractRow, GetOrderBy()
            {
                //oldArticles = oldArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Article_number).ToList();
                //oldEducationPortals = oldEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Article_number).ToList();
                //remEducationPortals = remEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Article_number).ToList();
            }
            else if (usr.AvtalSortera == 4) //Classification, Article_Sort_number
            {
                oldArticles = oldArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(m => m.Article_Sort_number).ToList();
                oldEducationPortals = oldEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(m => m.Article_Sort_number).ToList();
                remEducationPortals = remEducationPortals.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(m => m.Article_Sort_number).ToList();
            }

            ViewData.Add("OldEducationPortals", oldEducationPortals);
            ViewData.Add("OldArticles", oldArticles);
            ViewData.Add("RemEducationPortals", remEducationPortals);
            ViewData.Add("CtrResign", ctrResign);

            view_Reminder vR = new view_Reminder();

            if(contract.Customer != null)
            {
                var remindExist = vR.checkIfReminderPerCustomer(contract.Customer, System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);
                ViewData.Add("ShowReminderButton", remindExist.CompareTo("-1") == 0 ? false : true);
            }
            else
            {
                ViewData.Add("ShowReminderButton", false);
            }

            //Här styrs sorteringen av artiklarna ut på avtalet av aktuella artiklar.
            if (usr.AvtalSortera == 1) //Article name
            {
                articles = articles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                remArticles = remArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
            }
            else if (usr.AvtalSortera == 2) //Classification, Article name
            {
                articles = articles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
                remArticles = remArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Module).ToList();
            }
            else if (usr.AvtalSortera == 3 && contract._ContractConsultantRows != null) //Classification, Article-no. Denna sortering styrs från view_ContractRow, GetOrderBy()
            {
                //articles = articles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Article_number).ToList();
                //remArticles = remArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenBy(m => m.Article_number).ToList();

                contract._ContractConsultantRows = contract._ContractConsultantRows.OrderBy(a => a.Code).ToList();
            }
            else if (usr.AvtalSortera == 4) //Classification, Article sort number
            {
                articles = articles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(m => m.Article_Sort_number).ToList();
                remArticles = remArticles.OrderBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(m => m.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(m => m.Article_Sort_number).ToList();
                foreach (var system in articleSystemDic)
                {
                    //.ThenByDescending(a => a.Article_Sort_number > 0) => Vi vill ha null- och 0-poster sist i sorteringen
                    var sortedList = new List<dynamic>();
                    sortedList.AddRange(system.Value);
                    system.Value.Clear();
                    system.Value.AddRange(sortedList.OrderBy(a => a.ModuleType).ThenBy(a => a.Price_type).ThenBy(a => a.Sort_number).ThenBy(a => a.Classification).ThenByDescending(a => a.Article_Sort_number > 0).ThenBy(a => a.Article_Sort_number).ToList());
                }

                contract._ContractConsultantRows = contract._ContractConsultantRows.OrderByDescending(a => a._SortOrder > 0).ThenBy(a => a._SortOrder).ToList();
            }

            ViewData.Add("EducationPortals", educationPortals);
            ViewData.Add("Articles", articles);
            ViewData.Add("RemArticles", remArticles);

            ViewData.Add("ArticleSystemDictionary", articleSystemDic.OrderBy(d => d.Value.First().Price_type).ThenBy(d => d.Value.First().Sort_number).ThenBy(d => d.Value.First().Classification).ThenBy(d => d.Value.First().Module).ToList());
            ViewData.Add("ArticleAndServiceDictionary", articleAndServicesDic.OrderBy(d => d.Value.First().ModuleType).ThenBy(d => d.Value.First().Price_type).ThenBy(d => d.Value.First().Sort_number).ThenBy(d => d.Value.First().Classification).ThenBy(d => d.Value.First().Module).ToList());

            List<dynamic> eduOptions = new List<dynamic>();
            List<dynamic> options = new List<dynamic>();
            SortedList<String, List<dynamic>> optionSystemList = new SortedList<String, List<dynamic>>();

            foreach (view_ContractOption option in view_ContractOption.getAllOptions(urlContractId, urlCustomer))
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + option.Article_number);
                dynamic article = new ExpandoObject();
                article.Article_number = module.Article_number;
                article.Contract_id = option.Contract_id;
                article.Module = module.Module;
                article.System = module.System;

                view_Sector sector = new view_Sector();
                sector.Select("System=" + module.System + " AND Classification=" + module.Classification);

                article.Price_type = sector.Price_type;

                article.Classification = module.Classification;
                article.License = option.License;
                article.Maintenance = option.Maintenance;

                //options.Add(article);
                if (!optionSystemList.ContainsKey(article.System))
                {
                    optionSystemList.Add(article.System, new List<dynamic> { article });
                }
                else
                {
                    optionSystemList[article.System].Add(article);
                }
            }

            ViewData.Add("OptionSystemList", optionSystemList.OrderBy(d => d.Value.First().Price_type).ToList());

            ViewData.Add("Options", options.OrderBy(a => a.Price_type).ThenBy(a => a.System).ThenBy(a => a.Classification).ThenBy(a => a.Article).ToList());
            //ViewData.Add("Options", options.OrderBy(a => a.Price_type).ThenBy(a => a.System).ThenBy(a => a.Classification).ThenBy(a => a.Article_number).ToList());
            ViewData.Add("EducationalOptions", eduOptions);

            view_CustomerContact cc = new view_CustomerContact();
            cc.Select("Customer = '" + contract.Customer + "' AND Contact_person = '" + contract.Contact_person + "'");
            ViewData.Add("CustomerContact", cc);

            view_Customer customer = new view_Customer();
            customer.Select("Customer = '" + contract.Customer + "'");
            customer.Select("ID=" + customer._ID);
            ViewData.Add("Customer", customer);

            if(contract.Sign != null)
            {
                view_User user = new view_User();
                user.Select("Sign = " + contract.Sign);
                //if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                //    user = System.Web.HttpContext.Current.GetUser();
                //else
                //{
                //    List<view_User> users = new List<view_User>();
                //    foreach(String name in customer._Representatives)
                //    {
                //        view_User rep = new view_User();
                //        rep.Select("Sign=" + name);
                //        users.Add(rep);
                //    }
                //    if (users.Count > 0)
                //    {
                //        List<view_User> tempUsers = users.Where(u => u.Area == contract.Area).ToList();
                //        if(tempUsers.Count > 0)
                //            user = tempUsers.First();
                //        else
                //            user = System.Web.HttpContext.Current.GetUser();
                //    }
                //    else
                //        user = System.Web.HttpContext.Current.GetUser();
                //}

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

            }
            else
            {
                ViewData.Add("OurCity", "");
            }


            this.ViewData.Add("Services", view_Module.getAllModules(false, 2).Where(w => !w.Expired.Value)); //Exkludera utgångna tjänster

            List<view_CustomerOffer> openOffers = view_CustomerOffer.getAllCustomerOffers(customer.Customer)
                .Where(o => o.Offer_status == "Öppen" && o.Area == contract.Area)
                .ToList();

            List<view_OfferRow> modules = new List<view_OfferRow>();
            List<view_ConsultantRow> services = new List<view_ConsultantRow>();

            foreach (view_CustomerOffer offer in openOffers)
            {
                foreach (view_OfferRow offerRow in offer._OfferRows)
                {
                    //if (!modules.Contains(offerRow))
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
                properties = typeof(view_Contract).GetProperties().Where(   p => 
                        p.Name == "Contract_id" || p.Name == "Title" || p.Name == "Status" || p.Name == "Main_contract_id" || p.Name == "Contract_type" ||
                        p.Name == "Term_of_notice" || p.Name == "Status" || p.Name == "CRM_id" || p.Name == "Valid_from" || p.Name == "Valid_through" ||
                        p.Name == "Extension" || p.Name == "Expire" || p.Name == "Observation" || p.Name == "Note" ||
                        p.Name == "Sign" || p.Name == "Area" || p.Name == "Summera" || p.Name == "Monthly_fee_from"
                    ).ToList();
            }
            else
            {
                properties = typeof(view_Contract).GetProperties().Where(p => p.Name == "Contract_id" || p.Name == "Status" || p.Name == "CRM_id" || p.Name == "Observation" || p.Name == "Note" || p.Name == "Sign" || p.Name == "Summera").ToList();
            }
            this.ViewData.Add("TableItems", properties);
            //this.ViewData.Add("Statuses", GetStatuses());
            this.ViewData.Add("Statuses", (new SelectOptions<view_Contract>()).GetSelectOptions("Status"));
            //this.ViewData.Add("ContractTypes", GetContractTypes());
            this.ViewData.Add("ContractTypes", (new SelectOptions<view_Contract>()).GetSelectOptions("Contract_type"));
            List<String> mainContracts = view_Contract.GetContracts(customer.Customer).Where(c => c.Contract_type == "Huvudavtal").Select(c => c.Contract_id).ToList();
            this.ViewData.Add("MainContracts", mainContracts);
            this.ViewData.Add("Users", view_User.getAllUsers().Select(u => u.Sign));

            ViewData.Add("MainContractTitle", this.GetTopHead(urlCustomer, urlContractId));
            ViewData.Add("Prolog", this.GetProlog(urlCustomer, urlContractId));
            ViewData.Add("Epilog", this.GetEpilog(urlCustomer, urlContractId));
            ViewData.Add("ModuleText", this.GetModuleText(urlCustomer, urlContractId));
            ViewData.Add("MainContractTemplates", this.GetAllMainContractTemplates());
            ViewData.Add("Contract_Description", this.GetContract_Description(urlCustomer, urlContractId));
            ViewBag.Area = System.Web.HttpContext.Current.GetUser().Area;

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

            string cusomtSwitches = string.Format("--print-media-type --margin-top 18 --margin-bottom 20 --header-spacing 2 --header-html \"{1}\" --footer-html \"{0}\" ", footerFilePath, headerFilePath);
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = cusomtSwitches;

            // Set title and file names.
            String fileName = (new FileLocationMapping(user, (view_Contract)ViewData["CustomerContract"])).GetFilePath() + ".pdf";
            fileName = fileName.Replace(",", "");

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
            //if (user.Use_logo)
            //    content += @"<div class='footer-logo'>
            //                <img src='../../Content/img/tieto-logo.png' alt='tieto-logo' />
            //            </div>";
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
                            <img src='../../Content/img/TE-Lockup-RGB-BLUE.png' />
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
            String our_sign = Request.Form["our_sign"];

            if (customer == "" || customer == null)
            {
                List<String> customerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
                if (customerNames.Count <= 0)
                    customer = "";
            }

            List<view_Contract> customerContracts;
            if(customer != "*")
                customerContracts = view_Contract.GetContracts(customer, true);
            else
                customerContracts = view_Contract.GetContracts(true);

            List<Dictionary<String, dynamic>> contracts = new List<Dictionary<String, dynamic>>();

            view_User user = System.Web.HttpContext.Current.GetUser();
            List<view_Customer> vCustomers = new List<view_Customer>();
            foreach (view_Contract contract in customerContracts.Where(c => (c.Sign == our_sign && our_sign != "*") || (c.Sign == c.Sign && our_sign == "*")))
            {
                view_Customer vCustomer;
                if (vCustomers.Any(c => c.Customer == contract.Customer))
                    vCustomer = vCustomers.Find(c => c.Customer == contract.Customer);
                else
                {
                    vCustomer = new view_Customer("Customer='" + contract.Customer + "'");
                    vCustomers.Add(vCustomer);
                }
                if(user.IfSameArea(contract.Area) && (vCustomer._Representatives.Contains(user.Sign) || user.User_level == 1))
                {
                    Dictionary<String, dynamic> variables = new Dictionary<String, dynamic>();
                    foreach (System.Reflection.PropertyInfo pi in contract.GetType().GetProperties())
                    {

                        if (!pi.Name.StartsWith("_") && !this.skipProp.Contains(pi.Name))
                        {
                            if (pi.Name == "Extension")
                                variables.Add(pi.Name, contract.getStringExtension());
                            else if (pi.Name == "Term_of_notice")
                                variables.Add(pi.Name, contract.getStringTON());
                            else
                                variables.Add(pi.Name, pi.GetValue(contract));
                        }

                    }
                    variables.Add("_HashtagList", contract.HashtagsAsString());
                    contracts.Add(variables);
                }
                
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
                    customer.Select("Customer = '" + customerString + "'");
                    contractHead.Address = customer.Address;
                    contractHead.City = customer.City;
                    contractHead.Buyer = customer.UseShortNameAsReceiver == 1 ? customer.Short_name : customer.Customer;
                    contractHead.Zip_code = customer.Zip_code;
                    contractHead.Corporate_identity_number = customer.Corporate_identity_number;
                    contractHead.Administration = customer.Administration;

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

        /// <summary>
        /// Creates a contract
        /// </summary>
        /// <returns></returns>
        public string Insert()
        {
            try
            {
                string json = Request.Form["json"];
                view_Contract a = null;
                try
                {
                    a = (view_Contract)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Contract));
                    a.ParseHashtags(Request["hashtags"]);
                    int i = 1;
                    bool foundIndex = false;
                    string contractId = "";
                    view_Contract contract = new view_Contract();
                    while(!foundIndex) // make sure that we will get a unique contract id
                    {
                        if (a.Is(ContractType.ModuleTermination))
                        {
                            contractId = "XIT " + System.Web.HttpContext.Current.GetUser().Sign + " " + DateTime.Now.ToShortDateString() + " " + i.ToString("00");
                        }
                        else
                        {
                            contractId = System.Web.HttpContext.Current.GetUser().Area + " " + System.Web.HttpContext.Current.GetUser().Sign + " " + DateTime.Now.ToShortDateString() + " " + i.ToString("00");
                        }

                        contract = new view_Contract();
                        contract.Select("Contract_id = '" + contractId + "'");

                        if(contract.Contract_id == null)
                            foundIndex = true;

                        i++;
                    }

                    a.Contract_id = contractId;
                    a.Sign = System.Web.HttpContext.Current.GetUser().Sign;
                    a.Area = System.Web.HttpContext.Current.GetUser().Area;
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
                contractHead.Buyer = customer.UseShortNameAsReceiver == 1 ? customer.Short_name : customer.Customer;
                contractHead.Customer = customer.Customer;
                contractHead.Zip_code = customer.Zip_code;
                contractHead.Corporate_identity_number = customer.Corporate_identity_number;
                contractHead.Contact_person = a.Contact_person;
                //contractHead.Customer_sign = a.Contact_person.Substring(0, Math.Min(a.Contact_person.Length, 50));
                contractHead.Customer_sign = "";
                contractHead.Our_sign = System.Web.HttpContext.Current.GetUser().Name.Substring(0, Math.Min(System.Web.HttpContext.Current.GetUser().Name.Length, 50));
                contractHead.Administration = customer.Administration;

                using (var scope = TransactionHelper.CreateTransactionScope())
                {
                    contractHead.Insert();

                    a.Insert();

                    new view_AuditLog().Write("C", "view_Contract", a._ID.ToString(), a.Contract_id + ", " + a.Customer);

                    scope.Complete();
                }

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
                    cRow.Alias = article["Alias"].ToString();

                    cRow.Delete("Article_number = " + cRow.Article_number + " AND Contract_id = '" + cRow.Contract_id + "' AND Customer = '" + cRow.Customer + "'");
                    cRow.Insert();

                    //Läs upp eventuell modultext
                    view_ModuleText offerModuleText = new view_ModuleText();
                    offerModuleText.Select("Type = 'O' AND TypeId = " + article["Offer_number"] + " AND ModuleId = " + module.Article_number.ToString());

                    if(!string.IsNullOrEmpty(offerModuleText.Description)) //Vi har en modultext på offerten, spara den till kontraktet!
                    {
                        //Delete-insert (om modultexten från offerten har ändrats)
                        view_ModuleText contractModuleText = new view_ModuleText();
                        contractModuleText.Delete("Type = 'A' AND TypeId = " + contract._ID + " AND ModuleId = " + ((int)module.Article_number).ToString());
                        InsertModuleText(offerModuleText.Description, "A", contract._ID, (int)module.Article_number);
                    }
                }

                foreach(dynamic serviceObj in services)
                {
                    view_Module service = new view_Module();
                    service.Select("Article_number =  " + serviceObj["code"]);

                    view_ContractConsultantRow ccRow = new view_ContractConsultantRow();

                    ccRow.Code = int.Parse(service.Article_number.ToString());
                    ccRow.Customer = contract.Customer;
                    ccRow.Contract_id = contract.Contract_id;
                    ccRow.Created = DateTime.Now;
                    ccRow.Amount = serviceObj["amount"];
                    ccRow.Total_price = serviceObj["total_price"];

                    ccRow.Delete("Code = " + ccRow.Code + " AND Contract_id = '" + ccRow.Contract_id + "' AND Customer = '" + ccRow.Customer + "'");
                    ccRow.Insert();

                    //Läs upp eventuell modultext
                    view_ModuleText offerModuleText = new view_ModuleText();
                    offerModuleText.Select("Type = 'O' AND TypeId = " + serviceObj["offer_number"].ToString() + " AND ModuleId = " + service.Article_number.ToString());

                    if (!string.IsNullOrEmpty(offerModuleText.Description)) //Vi har en modultext på offerten, spara den till kontraktet!
                    {
                        //Delete-insert (om modultexten från offerten har ändrats)
                        view_ModuleText contractModuleText = new view_ModuleText();
                        contractModuleText.Delete("Type = 'A' AND TypeId = " + contract._ID + " AND ModuleId = " + service.Article_number.ToString());
                        InsertModuleText(offerModuleText.Description, "K", contract._ID, int.Parse(service.Article_number.ToString()));
                    }
                }
                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                string moduleInfo = updateDescriptions(contract.Customer, contract.Contract_id, false);

                view_ContractText contractText = new view_ContractText();
                contractText.Select("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                if (contractText.Contract_id == null && contract.Contract_type.CompareTo("Huvudavtal") != 0)
                {
                    //Tilläggsavtal. Denna rad för kontraktets texter skapas i vanliga fall via menyn 'Edit/Text templates', 
                    //men när vi lägger till artiklar i ett nytt tilläggsavtal så skapar vi upp den här för att kunna få in modultexterna på en gång.
                    contractText.Contract_id = contract.Contract_id;
                    contractText.Customer = contract.Customer;
                    contractText.Contract_type = "Tilläggsavtal";
                    contractText.Module_info = moduleInfo;
                    contractText.Insert();
                }
                else
                {
                    contractText.UpdateModuleInfo(contract.Customer, contract.Contract_id, moduleInfo, contract.Contract_type);
                }

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

        /// <summary>
        /// Puts articles as REMOVED on contracts for customer
        /// </summary>
        /// <returns></returns>
        public string RemoveItemsFromContracts()
        {
            try
            {
                String removedFromContractId = Request.Form["contract-id"];
                List<dynamic> modules = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(Request.Form["modules"], typeof(List<dynamic>));

                //Modules to be marked as REMOVED
                foreach (dynamic article in modules)
                {
                    view_ContractRow contractRow = new view_ContractRow();
                    contractRow.Select("Customer = '" + article["Customer"] + "' AND Contract_id = '" + article["Contract_id"] + "' AND Article_number = " + article["Article_number"]);
                    contractRow.RemovedFromContractId = removedFromContractId;

                    decimal license;
                    decimal maintenance;
                    if (decimal.TryParse(article["License"], out license))
                    {
                        if(contractRow.License != license)
                        {
                            contractRow.License = license;
                        }
                    }

                    if (decimal.TryParse(article["Maintenance"], out maintenance))
                    {
                        if(contractRow.Maintenance != maintenance)
                        {
                            contractRow.Maintenance = maintenance;
                        }
                    }

                    if(article["Alias"] != null && article["Alias"] != contractRow.Alias)
                    {
                        contractRow.Alias = article["Alias"];
                    }

                    //Just an Update() does not work on view_ContractRow so I made a specifik update method...
                    contractRow.UpdateContractRowAsRemoved();

                    //Add moduleinfo texts also, as we want to have these texts under the "removed articles" section in the contract
                    var contract = new view_Contract("Customer = '" + article["Customer"] + "' AND Contract_id = '" + removedFromContractId + "'");
                    if(contract._ID > 0)
                    {
                        view_Module module = new view_Module();
                        module.Select("Article_number =  " + article["Article_number"]);

                        //First try to find existing moduletext for this article and contract.
                        var moduleText = new view_ModuleText();
                        moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + module.Article_number.ToString());
                        if (moduleText._ID > 0) //Vi har en modultext
                        {
                            if(moduleText.Deleted)
                            {
                                moduleText.Deleted = false;
                                moduleText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + module.Article_number.ToString());
                            }
                        }
                        else
                        {
                            InsertModuleText(module.Contract_description, "A", contract._ID, (int)module.Article_number);
                        }
                    }
                }

                return "1";
            }
            catch (Exception ex)
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

                    //Nedanstående 2 rader borttagna, då jag inte kan se att de används. Bara genererar en massa onödiga läsningar mot databasen /Mats N 2014-08-25 
                    //List<Dictionary<String, object>> modulePrices = view_Module.getModuleWithCorrectPrice(module.System, contract.Customer, module.Classification);
                    //Dictionary<String, object> modulePrice = modulePrices.FirstOrDefault(m => module.Article_number == Convert.ToSingle(m["Article_number"]));

                    view_ContractRow cRow = new view_ContractRow();
                    cRow.Article_number = Convert.ToInt32(module.Article_number);
                    cRow.Customer = contract.Customer;
                    cRow.Contract_id = contract.Contract_id;
                    cRow.Created = DateTime.Now;
                    decimal License = 0;
                    decimal Maintenance = 0;
                    
                    if (int.Parse(article["Discount_type"]) != 1)
                    {
                        if (article.ContainsKey("License"))
                            License = Decimal.Parse(article["License"].ToString().Replace(",", "."), NumberFormatInfo.InvariantInfo);
                        Maintenance = Decimal.Parse(article["Maintenance"].ToString().Replace(",", "."), NumberFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        String temp = article["License"].ToString().Replace(".", ",").Replace("%", "");
                        License = Decimal.Parse(temp);
                    }
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
                    view_Module service = new view_Module();
                    service.Select("Article_number = " + serviceObj["code"]);

                    view_ContractConsultantRow ccRow = new view_ContractConsultantRow();

                    ccRow.Code = int.Parse(service.Article_number.ToString());
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
            catch (Exception e)
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
                if (!String.IsNullOrEmpty(cRow.Alias))
                    module.Module = cRow.Alias;

                var obj = new
                {
                    Module = module.Module,
                    Article_number = module.Article_number,
                    Maintenance = cRow.Maintenance,
                    License = cRow.License,
                    Price_category = module.Price_category,
                    Maint_price_category = module.Maint_price_category,
                    System = module.System,
                    Multiple_type = module.Multiple_type,
                    Rewritten = cRow.Rewritten,
                    Removed = cRow.Removed,
                    NewMod = cRow.New,
                    Contract_id_key = cRow.Contract_id, //Could be module from other contract if it's been removed...
                    Removed_from_contract_id = cRow.RemovedFromContractId,
                    Rowtype = cRow.Removed.HasValue && cRow.Removed.Value ? "2" : cRow.Rewritten.HasValue && cRow.Rewritten.Value ? "1" : "3"
                };
                
                

                if(System.Web.HttpContext.Current.GetUser().Area == module.Area || System.Web.HttpContext.Current.GetUser().Area == "*")
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
                view_Module service = new view_Module();
                service.Select("Article_number = " + ccRow.Code);
                if (!String.IsNullOrEmpty(ccRow.Alias))
                {
                    service.Module = ccRow.Alias;
                }

                //Läs upp eventuell modultext för respektive tjänst för att veta om defaulttext eller modultext ska läggas till
                //då man adderar tjänsten till kontraktet.
                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleType = 'K' AND ModuleId = " + service.Article_number.ToString());


                var obj = new
                {
                    Code = service.Article_number,
                    Module = service.Module,
                    Price = ccRow.Total_price / ccRow.Amount,
                    Amount = ccRow.Amount,
                    Total = ccRow.Total_price,
                    Contract_Description = service.Contract_description,
                    Contract_id = contract._ID,
                    Module_text_id = moduleText._ID,
                    Module_type = "K",
                    Article_number = service.Article_number
                };

                services.Add(obj);
            }

            return (new JavaScriptSerializer()).Serialize(services.OrderBy(s => s.Module));
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
                //Unvalidated - vi skickar nu med html-kod (contract_description) som anses "farlig" och genererar ett fel annars
                String selectedArticles = HttpContext.Request.Unvalidated.Form["Object"];
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
                    //Vi tar bort automatisk borttagning av kopplade tjänster. Det ställer till det genom att dom inte läggs till igen då "automapping" inte är "true" då man kommer in en andra gång i dialogen...
                    ////Kolla först om det finns kopplade tjänster som också ska tas bort.
                    //var mappedModuleList = view_ModuleModule.getAllChildModules(cr.Article_number);

                    //foreach (var mappedModule in mappedModuleList)
                    //{
                    //    if (mappedModule.Article_number > 0 && mappedModule.Module_type == 2)
                    //    {
                    //        view_ContractConsultantRow consultantRow = new view_ContractConsultantRow();
                    //        if(consultantRow.Select("Contract_id = " + contract.Contract_id + " AND Customer = " + contract.Customer + " AND Code = " + ((int)mappedModule.Article_number).ToString()))
                    //        {
                    //            if (consultantRow.Amount > 1)
                    //            {
                    //                //Minska med 1
                    //                var pricePerUnit = consultantRow.Total_price / consultantRow.Amount;
                    //                consultantRow.Amount = consultantRow.Amount - 1;
                    //                consultantRow.Total_price -= pricePerUnit;
                    //                consultantRow.Update("Contract_id = " + contract.Contract_id + " AND Customer = " + contract.Customer + " AND Code = " + ((int)mappedModule.Article_number).ToString());
                    //            }
                    //            else
                    //            {
                    //                //Ta bort post
                    //                consultantRow.Delete("Contract_id = " + contract.Contract_id + " AND Customer = " + contract.Customer + " AND Code = " + ((int)mappedModule.Article_number).ToString());

                    //                //Ta även bort eventuell modultext (deleted = true)
                    //                view_ModuleText moduleServiceText = new view_ModuleText();
                    //                moduleServiceText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + ((int)mappedModule.Article_number).ToString());
                    //                if (moduleServiceText._ID > 0) //Vi har en modultext
                    //                {
                    //                    //Den ska delete-markeras
                    //                    moduleServiceText.Deleted = true;
                    //                    moduleServiceText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + ((int)mappedModule.Article_number).ToString());
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //if (cr.Rewritten == true)
                    //    rewrittens.Add(cr);
                    if(string.IsNullOrEmpty(cr.RemovedFromContractId)) //Ta ej bort artiklar från andra kontrakt (som är deletade (Removed) från detta kontrakt)
                    {
                        cr.Delete("Customer = '" + cr.Customer + "' AND Contract_id = '" + cr.Contract_id + "' AND Article_number = " + cr.Article_number);

                        //Ta även bort eventuell modultext (deleted = true)
                        view_ModuleText moduleText = new view_ModuleText();
                        moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + cr.Article_number.ToString());
                        if (moduleText._ID > 0) //Vi har en modultext
                        {
                            //Den ska delete-markeras
                            moduleText.Deleted = true;
                            moduleText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + cr.Article_number.ToString());
                        }
                    }
                }

                foreach (Dictionary<String, Object> dict in list)
                {
                    object test;
                    if (!dict.TryGetValue("Removed_from_contract_id", out test) || string.IsNullOrEmpty(dict["Removed_from_contract_id"].ToString()))
                    {
                        int Article_number = Convert.ToInt32(dict["Article_number"]);
                        decimal License = 0;
                        decimal Maintenance = 0;
                        if(dict["Discount_type"].GetType() == typeof(string))
                        {
                            dict["Discount_type"] = (string)dict["Discount_type"] == "undefined" ? 0 : dict["Discount_type"];
                        }

                        if ((int)dict["Discount_type"] != 1)
                        {
                            if (dict.Keys.Contains("License"))
                                License = Decimal.Parse(dict["License"].ToString().Replace(",", "."), NumberFormatInfo.InvariantInfo);
                            Maintenance = Decimal.Parse(dict["Maintenance"].ToString().Replace(",", "."), NumberFormatInfo.InvariantInfo);
                        }
                        else
                        {
                            if (dict.Keys.Contains("License"))
                                License = Decimal.Parse(dict["License"].ToString().Replace(".", ",").Replace("%", ""));
                            Maintenance = Decimal.Parse(dict["Maintenance"].ToString().Replace(".", ",").Replace("%", ""));
                        }
                        int RowType = Convert.ToInt32(dict["Rowtype"]);

                        var automapping = dict["Automapping"] != null ? Convert.ToBoolean(dict["Automapping"]) : false;

                        view_ContractRow contractRow = new view_ContractRow();
                        contractRow.Customer = contract.Customer;
                        contractRow.Contract_id = contract.Contract_id;
                        contractRow.Article_number = Article_number;
                        contractRow.License = Convert.ToDecimal(License);
                        contractRow.Maintenance = Convert.ToDecimal(Maintenance);
                        contractRow.Alias = dict["Alias"].ToString();
                        contractRow.IncludeDependencies = automapping;

                        if (RowType == 3)
                        {
                            contractRow.New = false;
                            //if (urlctrResign == "True") //Ska bli nytt även i nya huvudavtal och tilläggsavtal...
                            {
                                contractRow.New = true;
                            }
                            contractRow.Rewritten = false;
                            contractRow.Removed = false;
                        }
                        if (RowType == 2)
                        {
                            contractRow.New = false;
                            contractRow.Rewritten = true;
                            contractRow.Removed = true;
                        }
                        if (RowType == 1)
                        {
                            contractRow.New = false;
                            contractRow.Rewritten = true;
                            contractRow.Removed = false;
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
                        contractRow.Insert();

                        //Eventuellt ska vi ta tillbaka en tidigare deletad Modultext
                        view_ModuleText moduleText = new view_ModuleText();
                        moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + contractRow.Article_number.ToString());

                        if (moduleText._ID > 0)
                        {
                            //Den ska avmarkeras FRÅN deleted.
                            moduleText.Deleted = false;
                            moduleText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + contractRow.Article_number.ToString());
                        }

                        if (automapping)
                        {
                            //Kopplade moduler ska läggas in i kontraktet
                            var mappedModuleList = view_ModuleModule.getAllChildModules(Article_number);

                            foreach (var mappedModule in mappedModuleList)
                            {
                                if (mappedModule.Article_number > 0 && mappedModule.Module_type == 2)
                                {
                                    bool avoidInsert = false;
                                    view_ContractConsultantRow consultantRow = new view_ContractConsultantRow();

                                    try
                                    {
                                        consultantRow.Contract_id = contract.Contract_id;
                                        consultantRow.Customer = contract.Customer;
                                        consultantRow.Code = (int)mappedModule.Article_number;
                                        consultantRow.Amount = 1;
                                        consultantRow.Total_price = mappedModule.Price_category;
                                        consultantRow.Created = DateTime.Now;
                                        consultantRow.Alias = mappedModule.Module;
                                        consultantRow.Insert();
                                    }
                                    catch (Exception)
                                    {
                                        if(mappedModule.Multiple_type == 1)
                                        {
                                            //Already exist on contract -> Räkna upp Amount!
                                            if (consultantRow.Select("Contract_id = " + contract.Contract_id + " AND Customer = " + contract.Customer + " AND Code = " + ((int)mappedModule.Article_number).ToString()))
                                            {
                                                var pricePerUnit = consultantRow.Total_price / consultantRow.Amount;
                                                consultantRow.Amount = consultantRow.Amount + 1;
                                                consultantRow.Total_price = consultantRow.Amount * pricePerUnit;
                                                consultantRow.Update("Contract_id = " + contract.Contract_id + " AND Customer = " + contract.Customer + " AND Code = " + ((int)mappedModule.Article_number).ToString());
                                            }
                                        }
                                        else
                                        {
                                            //Avod adding service + moduletext
                                            avoidInsert = true;
                                        }
                                    }

                                    //Lägg till eventuella beskrivningstexter i view_ModuleText
                                    if (!avoidInsert && !string.IsNullOrEmpty(mappedModule.Contract_description))
                                    {
                                        //Delete-insert (om modultexten har ändrats)
                                        view_ModuleText contractModuleText = new view_ModuleText();
                                        contractModuleText.Delete("Type = 'A' AND TypeId = " + contract._ID + " AND ModuleId = " + ((int)mappedModule.Article_number).ToString());

                                        InsertModuleText(mappedModule.Contract_description, "K", contract._ID, (int)mappedModule.Article_number);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if(Convert.ToInt32(dict["Rowtype"]) == 1)
                        {
                            //Ångrat borttag av artikel i kopplat avtal.
                            var articleToReWrite = new view_ContractRow();
                            articleToReWrite.Select("Customer = '" + urlCustomer + "' AND Contract_id = '" + dict["Contract_id_key"] + "' AND Article_number = " + dict["Article_number"]);

                            //Städa bort modultexten från avtalet som tog bort modulen
                            var moduleText = new view_ModuleText();
                            moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + articleToReWrite.Article_number.ToString());
                            if (moduleText._ID > 0) //Vi har en modultext
                            {
                                moduleText.Delete("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + articleToReWrite.Article_number.ToString());
                            }

                            articleToReWrite.UpdateContractRowAsRewritten(); //Also removes the "removed from contract id" connection
                        }
                        else
                        {
                            //Do nothing
                        }
                    }
                }

                contract.Updated = System.DateTime.Now;
                contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");

                view_ContractText ctext = new view_ContractText();
                string moduleInfo = updateDescriptions(contract.Customer, contract.Contract_id, false);
                ctext.UpdateModuleInfo(contract.Customer, contract.Contract_id, moduleInfo, contract.Contract_type);

                return "1";
            }
            catch (Exception ex)
            {
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
            //Unvalidated - vi skickar nu med html-kod (contract_description) som anses "farlig" och genererar ett fel annars
            string obj = HttpContext.Request.Unvalidated.Form["object"];
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

                //Ta även bort eventuell modultext (deleted = true)
                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + cr.Code.ToString());
                if (moduleText._ID > 0) //Vi har en modultext
                {
                    //Den ska delete-markeras
                    moduleText.Deleted = true;
                    moduleText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + cr.Code.ToString());
                }
            }

            foreach (Dictionary<String, Object> dict in list)
            {
                int id = Convert.ToInt32(dict["id"]);
                int amount = Convert.ToInt32(dict["amount"]);
                int total = Convert.ToInt32(dict["total"]);
                String alias = "";
                if (dict.Keys.Contains("desc"))
                    alias = dict["desc"].ToString();

                view_ContractConsultantRow consultantRow = new view_ContractConsultantRow();
                consultantRow.Contract_id = contract.Contract_id;
                consultantRow.Customer = contract.Customer;
                consultantRow.Alias = alias;
                consultantRow.Code = id;
                consultantRow.Amount = amount;
                consultantRow.Total_price = total;
                consultantRow.Created = DateTime.Now;
                consultantRow.Insert();

                //Eventuellt ska vi ta tillbaka en tidigare deletad Modultext
                view_ModuleText moduleText = new view_ModuleText();
                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + consultantRow.Code.ToString());

                if (moduleText._ID > 0)
                {
                    //Den ska avmarkeras FRÅN deleted.
                    moduleText.Deleted = false;
                    moduleText.Update("Type = 'A' AND TypeId = " + contract._ID.ToString() + " AND ModuleId = " + consultantRow.Code.ToString());
                }
            }

            contract.Updated = System.DateTime.Now;
            contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");

            view_ContractText ctext = new view_ContractText();
            string moduleInfo = updateDescriptions(contract.Customer, contract.Contract_id, false);
            ctext.UpdateModuleInfo(contract.Customer, contract.Contract_id, moduleInfo, contract.Contract_type);

            return "1";
        }

        public String GetModules()
        {
            String customer = Request.Form["customer"];
            String system = Request.Form["System"];
            String classification = Request.Form["classification"];
            String ctr = Request.Form["contracttype"];
            String contractId = Request.Form["contractId"]; //För att kunna läsa upp kontraktet

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                String queryText = @"Select A.*, Case When T2.Maintenance = 0 Then T1.Maintenance Else IsNull(T2.Maintenance, T1.Maintenance) End As Maintenance, T1.License As License, IsNull(O.Text, '') as Module_status_txt
	                                    From (Select M.Article_number, M.Module, M.Price_category, M.Maint_price_category, M.System, M.Classification, M.Area, M.Fixed_price, M.Discount_type, 
                                                M.Discount, M.Comment, M.Multiple_type, C.Inhabitant_level, IsNull(M.Description,'') As Description, M.Module_status, IsNull(M.Contract_Description, '') AS Contract_Description, M.Module_type
					                                    from view_Module M, view_Customer C
					                                    Where C.Customer = @customer And M.Expired = 0) A
	                                    Left Join view_Tariff T1 On T1.Inhabitant_level = A.Inhabitant_level And T1.Price_category = A.Price_category
                                        Left Join view_Tariff T2 On T2.Inhabitant_level = A.Inhabitant_level And T2.Price_category = A.Maint_price_category
	                                    Left Join view_SelectOption O on O.Value = A.Module_status And O.Model = 'view_Module' And Property = 'Module_status'
	                                    Where A.System = @System AND A.Classification = @classification AND A.Module_type = 1 Order By Module";

                //Module_type = 1 = Modules
                //Module_type = 2 = Services

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
                command.Parameters.AddWithValue("@System", system);
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
                            result["Price_category"] = result["Price_category"].ToString().Replace(",", ".");
                            result["Maint_price_category"] = result["Maint_price_category"].ToString().Replace(",", ".");
                            result["System"] = result["System"].ToString();
                            if ((bool)result["Fixed_price"])
                            {
                                result["Maintenance"] = "0";
                                result["License"] = result["Price_category"];

                            }
                            if ((Byte)result["Discount"] == 1)
                            {
                                int length = result["Price_category"].ToString().Length;
                                result["Maintenance"] = result["Price_category"].ToString().Remove(length - 6, 5);
                                result["License"] = result["Price_category"].ToString().Remove(length - 6, 5);

                            }
                            view_ModuleDiscount moduleDiscount = new view_ModuleDiscount();
                            if (moduleDiscount.Select("Article_number=" + result["Article_number"].ToString()
                                + " AND Area=" + result["Area"].ToString()))
                            {
                                result["Maintenance"] = (decimal.Parse(result["Maintenance"].ToString()) * (1 - ((decimal)moduleDiscount.Maintenance_discount / 100))).ToString();
                                result["License"] = (decimal.Parse(result["License"].ToString()) * (1 - ((decimal)moduleDiscount.License_discount / 100))).ToString();
                                if (!String.IsNullOrEmpty(moduleDiscount.Alias))
                                    result["Module"] = moduleDiscount.Alias;
                            }
                            result["License"] = result["License"].ToString().Replace(",", ".");
                            result["Maintenance"] = result["Maintenance"].ToString().Replace(",", ".");

                            //Läser upp kontraktet för att sedan kunna läsa upp eventuella modultexter för att veta om 
                            //vi ska lägga till standardtext eller modultext då vi lägger till en modul till kontraktet
                            view_Contract contract = new view_Contract();
                            contract.Select("Contract_id = '" + contractId + "'");

                            if(contract._ID > 0)
                            {
                                view_ModuleText moduleText = new view_ModuleText();
                                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID + " AND ModuleType = 'A' AND ModuleId = " + result["Article_number"].ToString());
                                result.Add("Module_text_id", moduleText._ID);
                                result.Add("Contract_id", contract._ID);
                            }

                            view_User user = System.Web.HttpContext.Current.GetUser();

                            if (user.Area == result["Area"].ToString() || user.Area == "*")
                                resultList.Add(result);
                        }
                    }
                }
                Response.ContentType = "text/plain";

                List<view_ContractRow> usedArticles = view_ContractRow.GetValidContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();
                List<view_ContractRow> closedOrRewrittenArticles = view_ContractRow.GetClosedAndRewrittenContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();

                foreach (Dictionary<String, dynamic> kv in resultList)
                {
                    if (usedArticles.Any(cr => cr.Article_number == kv["Article_number"] && cr.Removed == false) && ctr != "M")
                        kv.Add("Used", true);
                    if (closedOrRewrittenArticles.Any(cr => cr.Article_number == kv["Article_number"] /*&& cr.Removed == false*/) && ctr != "M")
                        kv.Add("Expired", true);             
                    List<view_Module> dependencies = view_ModuleModule.getAllChildModules(int.Parse(kv["Article_number"].ToString()));
                    if(dependencies.Count > 0)
                    {
                        kv.Add("IncludeDependencies", true);
                        kv.Add("HasDependencies", true);
                        kv.Add("Dependencies", dependencies);
                    }
                    else
                    {
                        kv.Add("HasDependencies", false);
                        kv.Add("IncludeDependencies", false);
                    }
                }

                //Response.Charset = "UTF-8";
                // this.solve();
                String resultString = (new JavaScriptSerializer()).Serialize(resultList);
                return resultString;
            }
        }

        /// <summary>
        /// Search active modules in contract. Used for removal dialog in contract view
        /// </summary>
        /// <returns></returns>
        public string GetModulesForRemoval()
        {
            var customer = Request.Form["customer"];
            var searchtext = Request.Form["searchtext"];

            HashSet<view_ContractRow> customersModules = new HashSet<view_ContractRow>();

            foreach (view_Contract validContract in view_Contract.GetContracts(customer).Where(c => c.Status == "Giltigt"))
            {
                customersModules = new HashSet<view_ContractRow>(customersModules.Concat(validContract._ContractRows));
            }

            //ViewData.Add("ActiveCustomerModules", new HashSet<view_ContractRow>(customersModules.Where(w => !w.Removed.HasValue || (w.Removed.HasValue && !w.Removed.Value))));

            var resultString = new JavaScriptSerializer().Serialize(customersModules.Where(w => !w.Removed.HasValue || (w.Removed.HasValue && !w.Removed.Value)));
            return resultString;
        }

        /// <summary>
        /// Search modules for removal
        /// </summary>
        /// <returns></returns>
        public string SearchModulesForRemoval()
        {
            var customer = Request.Form["customer"];
            var searchtext = Request.Form["searchtext"];

            HashSet<view_ContractRow> customersModules = new HashSet<view_ContractRow>();

            foreach (view_Contract validContract in view_Contract.GetContracts(customer).Where(c => c.Status == "Giltigt"))
            {
                customersModules = new HashSet<view_ContractRow>(customersModules.Concat(validContract._ContractRows.Where(w => w.Alias.ToLower().Contains(searchtext.ToLower()))));
            }

            //ViewData.Add("ActiveCustomerModules", new HashSet<view_ContractRow>(customersModules.Where(w => !w.Removed.HasValue || (w.Removed.HasValue && !w.Removed.Value))));

            var resultString = (new JavaScriptSerializer()).Serialize(customersModules);
            return resultString;
        }

        public String GetModulesAll(){

            String customer = Request.Form["customer"];
            String searchtext = Request.Form["searchtext"];
            String ctr = Request.Form["contracttype"];
            String contractid = Request.Form["contractid"];
            String moduletype = Request.Form["moduletype"];

            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                String queryText = "";

                if (moduletype == "2") //Services
                {
                    queryText = @"Select M.Article_number, M.Module, M.Price_category, M.Maint_price_category, M.System, M.Classification, M.Comment, 
                                  M.Fixed_price, M.Multiple_type, M.Area, M.Discount_type, M.Discount, M.Module_status, IsNull(M.Contract_Description, '') AS Contract_Description, IsNull(M.[Description],'') As [Description] 
                                  From dbo.view_Module As M 
                                  Where M.Module_type = 2 And M.Expired = 0 And (Cast(M.Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(M.Article_number As Varchar(30)) Else @searchtext End Or 
                                  M.Module Like Case @searchtext When '' Then M.Module Else @searchtext End) 
                                  Order by M.Article_number asc";
                }
                else
                {
                    if (ctr == "Me")
                    {                //qry_GetModulesContractTermination + M.Contract_Description 
                        queryText = @"Select M.Article_number, M.Module, T1.License, Case When T2.Maintenance = 0 Then T1.Maintenance Else IsNull(T2.Maintenance, T1.Maintenance) End As Maintenance, M.Price_category, M.Maint_price_category, M.System, M.Classification, M.Comment, X.Customer, 
                                  M.Fixed_price, M.Multiple_type, M.Area, M.Discount_type, M.Discount, M.Module_status, IsNull(O.Text,'') as Module_status_txt, IsNull(M.Contract_Description, '') AS Contract_Description
                                  From dbo.view_Module As M 
                                  Inner Join dbo.view_Tariff T1 On M.Price_category = T1.Price_category
                                  Left Join dbo.view_Tariff T2 On M.Maint_price_category = T2.Price_category and T1.Inhabitant_level = T2.Inhabitant_level
                                  Inner Join (Select Customer, IsNull(Inhabitant_level, 1) As I_level From dbo.view_Customer) As X On X.I_level = T1.Inhabitant_level 
                                  Inner Join dbo.view_CustomerProductRow As R On R.Article_number = M.Article_number And R.Customer = X.Customer
                                  Left Join view_SelectOption O on O.Value = M.Module_status And O.Model = 'view_Module' And Property = 'Module_status'
                                  Where  (M.Module_type = @moduletype) And (M.Expired = 0) And (R.Discount_type = 0) And (R.status = 'Giltigt') and " + //#qry_GetModulesContractTermination

                                      @"X.Customer = @customer And (Cast(M.Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(M.Article_number As Varchar(30)) Else @searchtext End Or
                                  M.Module Like Case @searchtext When '' Then M.Module Else @searchtext End) 
                                  order by M.Module asc";
                    }
                    else
                    {                 //qry_GetModulesContractNormal + M.Contract_Description
                        queryText = @"Select M.Article_number, M.Module, T1.License, Case When T2.Maintenance = 0 Then T1.Maintenance Else IsNull(T2.Maintenance, T1.Maintenance) End As Maintenance, M.Price_category, M.Maint_price_category, M.System, M.Classification, M.Comment, X.Customer, 
                                  M.Multiple_type, M.Fixed_price, M.Area, M.Discount_type, M.Discount, IsNull(M.[Description],'') As [Description], M.Module_status, IsNull(O.Text,'') as Module_status_txt, IsNull(M.Contract_Description, '') AS Contract_Description
                                  From dbo.view_Module As M 
                                  Inner Join dbo.view_Tariff T1 On M.Price_category = T1.Price_category
                                  Left Join dbo.view_Tariff T2 On M.Maint_price_category = T2.Price_category and T1.Inhabitant_level = T2.Inhabitant_level
                                  Inner Join (Select Customer, IsNull(Inhabitant_level, 1) As I_level From dbo.view_Customer) As X On X.I_level = T1.Inhabitant_level 
                                  Left Join view_SelectOption O on O.Value = M.Module_status And O.Model = 'view_Module' And Property = 'Module_status'
                                  Where (M.Module_type = @moduletype) And (M.Expired = 0) and " + //#qry_GetModulesContractNormal

                                      @"X.Customer = @customer And (Cast(M.Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(M.Article_number As Varchar(30)) Else @searchtext End Or
                                  M.Module Like Case @searchtext When '' Then M.Module Else @searchtext End) 
                                  order by M.Module asc";
                    }
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
                command.Parameters.AddWithValue("@moduletype", moduletype);

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
                            result["Price_category"] = result["Price_category"].ToString().Replace(",", ".");
                            result["Maint_price_category"] = result["Maint_price_category"].ToString().Replace(",", ".");
                            result["System"] = result["System"].ToString();
                            if ((bool)result["Fixed_price"])
                            {
                                result["Maintenance"] = 0;
                                result["License"] = result["Price_category"];

                            }
                            if ((Byte)result["Discount"] == 1)
                            {
                                int length = result["Price_category"].ToString().Length;
                                result["Maintenance"] = result["Price_category"].ToString().Remove(length - 6, 5);
                                result["License"] = result["Price_category"].ToString().Remove(length - 6, 5);

                            }
                            view_ModuleDiscount moduleDiscount = new view_ModuleDiscount();
                            if (moduleDiscount.Select("Article_number=" + result["Article_number"].ToString()
                                + " AND Area=" + result["Area"].ToString()))
                            {
                                result["Maintenance"] = (decimal.Parse(result["Maintenance"].ToString()) * (1 - ((decimal)moduleDiscount.Maintenance_discount / 100))).ToString();
                                result["License"] = (decimal.Parse(result["License"].ToString()) * (1 - ((decimal)moduleDiscount.License_discount / 100))).ToString();
                                if (!String.IsNullOrEmpty(moduleDiscount.Alias))
                                    result["Module"] = moduleDiscount.Alias;
                            }
                            result["License"] = result["License"].ToString().Replace(",", ".");
                            result["Maintenance"] = result["Maintenance"].ToString().Replace(",", ".");

                            //Läser upp kontraktet för att sedan kunna läsa upp eventuella modultexter för att veta om 
                            //vi ska lägga till standardtext eller modultext då vi lägger till en modul till kontraktet
                            view_Contract contract = new view_Contract();
                            contract.Select("Contract_id = '" + contractid + "'");

                            if (contract._ID > 0)
                            {
                                view_ModuleText moduleText = new view_ModuleText();
                                moduleText.Select("Type = 'A' AND TypeId = " + contract._ID + " AND ModuleId = " + result["Article_number"].ToString());
                                result.Add("Module_text_id", moduleText._ID);
                                result.Add("Module_type", moduletype == "1" ? "A" : "K");
                                result.Add("Contract_id", contract._ID);
                            }

                            view_User user = System.Web.HttpContext.Current.GetUser();

                            if (user.Area == result["Area"].ToString() || user.Area == "*")
                                resultList.Add(result);
                        }
                    }
                }
                Response.ContentType = "text/plain";

                List<view_ContractRow> usedArticles = view_ContractRow.GetValidContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();
                List<view_ContractRow> closedOrRewrittenArticles = view_ContractRow.GetClosedAndRewrittenContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();

                foreach (Dictionary<String, dynamic> kv in resultList)
                {
                    if (usedArticles.Any(cr => cr.Article_number == kv["Article_number"] && cr.Removed == false) && ctr != "M")
                        kv.Add("Used", true);
                    if (closedOrRewrittenArticles.Any(cr => cr.Article_number == kv["Article_number"] /*&& cr.Removed == false*/) && ctr != "M")
                        kv.Add("Expired", true);

                    List<view_Module> dependencies = view_ModuleModule.getAllChildModules(int.Parse(kv["Article_number"].ToString()));
                    if (dependencies.Count > 0)
                    {                        
                        kv.Add("IncludeDependencies", true);
                        kv.Add("HasDependencies", true);
                        kv.Add("Dependencies", dependencies);
                    }
                    else
                    {
                        kv.Add("IncludeDependencies", false);
                        kv.Add("HasDependencies", false);
                    }                        

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

        public List<SelectListItem> GetAllSystemNames(String area)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area)
                 .DistinctBy(a => a.System)
                 .OrderBy(a => a.SortNo);
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();

            return returnList;
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
                String currentContent = Request.Form["contract-cont"];

                if (currentContent.StartsWith("true", StringComparison.OrdinalIgnoreCase) || selected.StartsWith("c", StringComparison.OrdinalIgnoreCase))
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
            }
            catch
            {
                return "0";
            }
           
        }

        public String GetModuleInfoTexts()
        {
            try
            {
                String customer = Request.Form["customer"];
                String contractId = Request.Form["contract-id"];

                view_ContractText text = new view_ContractText();
                text.Select("Contract_id = '" + contractId + "' AND Customer = '" + customer + "'");
                return (new JavaScriptSerializer()).Serialize(text);
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
            catch(Exception ex)
            {
                return "-1";
            }

        }

        public String GetMainContracts()
        {
            try
            {
                String customer = Request.Form["customer"];
                String our_sign = Request.Form["our_sign"];
                view_User user = System.Web.HttpContext.Current.GetUser();
                List<String> mainContracts = new List<String>();
                if (our_sign != "*")
                {
                    mainContracts = view_Contract.GetContracts(customer).Where(c => c.Is(ContractType.MainContract) && 
                        c.Status == "Giltigt" && c.Sign == our_sign && user.IfSameArea(c.Area)).Select(c => c.Contract_id).ToList();
                }
                else
                {
                   mainContracts = view_Contract.GetContracts(customer).Where(c => c.Is(ContractType.MainContract) && 
                        c.Status == "Giltigt" && user.IfSameArea(c.Area)).Select(c => c.Contract_id).ToList();
                }

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
                String hashtags = Request.Form["hashtags"];
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
                    if (entry.Value == null || String.IsNullOrEmpty(entry.Value.ToString()))
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
                a.ParseHashtags(hashtags);
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
                    return view_MainContractTemplate.GetMainContractTemplate(from).Prolog;
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
                    return view_MainContractTemplate.GetMainContractTemplate(from).Epilog;
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

        /// <summary>
        /// Sparar modultexter till view_ModuleText
        /// </summary>
        /// <returns></returns>
        public string SaveModuleInfoTexts()
        {
            try
            {
                string json = HttpContext.Request.Unvalidated.Form["object"];
                var contractId = 0;      //Unik id från view_Contract

                List<dynamic> map = null;
                try
                {
                    map = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(List<dynamic>));
                }
                catch (Exception)
                {
                    return "0";
                }

                if (map.Count == 0)
                {
                    return "0";
                }

                foreach (Dictionary<string, object> d in map)
                {
                    var amountOfModules = 0;

                    //Har vi med oss "Contract_description" så kommer vi direkt från addering av artikel eller tjänst - ej från Modul info dialogen
                    if(d.ContainsKey("Contract_description") && d.ContainsKey("Contract_id"))
                    {
                        var moduleTextId = d.ContainsKey("Module_text_id") ? Convert.ToInt32(d["Module_text_id"]) : 0;
                        object value;

                        contractId = Convert.ToInt32(d["Contract_id"]);

                        //ModulTextId = 0 (Finns inte sparad sedan tidigare)
                        try
                        {
                            if (moduleTextId == 0 && d.TryGetValue("Contract_description", out value) && !string.IsNullOrEmpty(d["Contract_description"].ToString())) //Skapa ny modultext från standardvärden på artikeln
                            {
                                view_ModuleText moduleText = InsertModuleText(d["Contract_description"].ToString(), d["Module_type"].ToString(), Convert.ToInt32(d["Contract_id"]), Convert.ToInt32(d["Article_number"]));
                                contractId = moduleText.TypeId;
                            }
                        }
                        catch (Exception)
                        {
                            //Troligtvis null-värde i Contract_description. Vi fortsätter loopen utan att skapa upp en modul-text
                            continue;
                        }
                    }
                    else
                    {
                        if(d.ContainsKey("moduleCount"))
                        {
                            amountOfModules = Convert.ToInt32(d["moduleCount"]) - 1;

                            for (int i = 1; i <= amountOfModules; i++)
                            {
                                view_ModuleText moduleText = new view_ModuleText();

                                moduleText.ChangedBy = System.Web.HttpContext.Current.GetUser().Sign;
                                moduleText.Changed = DateTime.Now;

                                var moduleTextId = 0;
                                int.TryParse(d["moduleTextId" + i.ToString()] != null ? d["moduleTextId" + i.ToString()].ToString() : "0", out moduleTextId);

                                if (moduleTextId > 0) //Existing ModuleText
                                {
                                    //Read row
                                    moduleText.Select("Id = " + moduleTextId.ToString());
                                }

                                foreach (KeyValuePair<string, object> entry in d)
                                {
                                    if (entry.Key != "moduleCount")
                                    {
                                        if (moduleText._ID == 0)
                                        {
                                            //Vi har flera modultexter i samma formulär med endast slutsiffra i id-namn som skiljer
                                            if (entry.Key.CompareTo("typeId" + i.ToString()) == 0)
                                            {
                                                moduleText.SetValue("TypeId", entry.Value);
                                            }
                                            else if (entry.Key.CompareTo("moduleId" + i.ToString()) == 0)
                                            {
                                                moduleText.SetValue("ModuleId", entry.Value);
                                            }
                                        }

                                        if (entry.Key.CompareTo("module-info-text" + i.ToString()) == 0)
                                        {
                                            moduleText.SetValue("Description", entry.Value);
                                        }
                                    }
                                }

                                if (moduleText._ID > 0)
                                    moduleText.Update("Id = " + moduleText._ID.ToString());
                                else
                                {
                                    //Endast vid INSERT
                                    moduleText = InsertModuleText(moduleText.Description, d["moduleType" + i.ToString()] != null ? d["moduleType" + i.ToString()].ToString() : "", moduleText.TypeId, moduleText.ModuleId);
                                }

                                contractId = moduleText.TypeId;
                            }
                        }
                    }
                }

                if (contractId > 0) //Vi har ett kontrakt
                {
                    //Uppdatera kontraktet med nya texterna.
                    view_Contract contract = new view_Contract("ID = " + contractId.ToString());

                    string moduleInfo = updateDescriptions(contract.Customer, contract.Contract_id, false);

                    view_ContractText contractText = new view_ContractText();
                    contractText.Select("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                    if (contractText.Contract_id == null && contract.Contract_type.CompareTo("Huvudavtal") != 0 )
                    {
                        //Tilläggsavtal. Denna rad för kontraktets texter skapas i vanliga fall via menyn 'Edit/Text templates', 
                        //men när vi lägger till artiklar i ett nytt tilläggsavtal så skapar vi upp den här för att kunna få in modultexterna på en gång.
                        //Detta var i grund och botten orsaken till de uppdateringsproblem vi hade i början med modultexterna... view_ContractText var inte uppskapad
                        contractText.Contract_id = contract.Contract_id;
                        contractText.Customer = contract.Customer;
                        contractText.Contract_type = "Tilläggsavtal";
                        contractText.Module_info = moduleInfo;
                        contractText.Insert();
                    }
                    else
                    {
                        contractText.UpdateModuleInfo(contract.Customer, contract.Contract_id, moduleInfo, contract.Contract_type);
                    }
                }

                return "1";
            }
            catch (Exception e)
            {
                var message = e.Message;
                return "-1";
            }
        }

        private static view_ModuleText InsertModuleText(string ContractDescription, string ModuleType, int contractId, int articleNumber)
        {
            view_ModuleText moduleText = new view_ModuleText
            {
                ChangedBy = System.Web.HttpContext.Current.GetUser().Sign,
                Changed = DateTime.Now,
                Description = ContractDescription,

                Type = "A", //Avtal

                ModuleType = ModuleType,
                TypeId = contractId,
                ModuleId = articleNumber,

                Order = 0, // Sorteringsordning. Lämnar den så länge

                CreatedBy = System.Web.HttpContext.Current.GetUser().Sign,
                Created = DateTime.Now
            };

            moduleText.Insert();
            return moduleText;
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
                    return view_MainContractTemplate.GetMainContractTemplate(from).TopTitle;
                }

            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// Hämtar totala modultexten för kontraktet
        /// </summary>
        /// <param name="customerP"></param>
        /// <param name="contractIdP"></param>
        /// <returns></returns>
        public string GetModuleText(string customerP = null, string contractIdP = null)
        {
            try
            {
                string customer = "";
                string contractId = "";
                string from = "";
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
                    string text = "";
                    var template = new view_ContractTemplate();
                    template.Select("Customer = '" + customer + "' AND Contract_id = '" + contractId + "'");
                    if (string.IsNullOrEmpty(template.ModuleText) && !String.IsNullOrEmpty(template.Text4))
                    {
                        text = ModuleTemplateToText(template);
                    }
                    else if (!string.IsNullOrEmpty(template.ModuleText))
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
                    return view_MainContractTemplate.GetMainContractTemplate(from).ModulText;
                }
            }
            catch
            {
                return "0";
            }
        }

        private List<view_MainContractTemplate> GetAllMainContractTemplates()
        {
            return view_MainContractTemplate.getAllMainContractTemplates();
        }

        public String GetContract_Description(String customerP = null, String contractIdP = null)
        {
            try
            {
                String text = "";
                view_ContractTemplate template = new view_ContractTemplate();
                template.Select("Customer = '" + customerP + "' AND Contract_id = '" + contractIdP + "'");
                if (!String.IsNullOrEmpty(template.Contract_Description))
                {
                    text = template.Contract_Description;
                }
                else
                {
                    text = "";
                }
                return text;
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

        public string DeleteContract()
        {
            try
            {
                string value = Request.Form["id"];
                string customer = Request.Form["customer"];
                view_Contract co = new view_Contract("Customer = '" + customer + "' AND Contract_id = '" + value + "'");
                //a.Select("Article_number = " + value);
                if (co.Status == "Makulerat")
                {
                    using (var scope = TransactionHelper.CreateTransactionScope())
                    {
                        co.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");
                        new view_AuditLog().Write("D", "view_Contract", co._ID.ToString(), "", co.Contract_id + ", " + co.Customer);

                        try
                        {
                            //Ta även bort från tabellen A_huvud (bug-fix)
                            view_ContractHead coh = new view_ContractHead();
                            coh.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");

                            //Ta även bort från tabellen A_avtalsrader (bug-fix)
                            view_ContractRow cr = new view_ContractRow();
                            cr.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");

                            //Ta även bort från tabellen A_konsultrader (bug-fix)
                            view_ContractConsultantRow ccr = new view_ContractConsultantRow();
                            ccr.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");

                            //Ta även bort från tabellen Option (bug-fix)
                            view_ContractOption copt = new view_ContractOption();
                            copt.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");

                            //Ta även bort från tabellen A_H_text (bug-fix)
                            view_ContractTemplate ct = new view_ContractTemplate();
                            ct.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");

                            //Ta även bort från tabellen A_text (bug-fix)
                            view_ContractText ctxt = new view_ContractText();
                            ctxt.Delete("Customer = '" + customer + "' AND Contract_id = '" + value + "'");
                        }
                        catch (Exception)
                        {
                            return "-2";
                        }

                        scope.Complete();
                    }
                }
                else
                    return "-1";

            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }

        [ValidateInput(false)]
        public string checkReminder()
        {
            string customer = Request.Form["customer"];
            string customerquerystr = Request.Form["customerquerystr"];
            string remindExist = "-1";
            string customerFromQuery = "";
            view_Reminder vR = new view_Reminder();

            if (!string.IsNullOrEmpty(customerquerystr))
            {
                customerFromQuery = WebUtility.HtmlDecode(customerquerystr);
            }

            if (!string.IsNullOrEmpty(customer))
            {
                if(customer.CompareTo(customerFromQuery) != 0)
                {
                    remindExist = vR.checkIfReminderPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);
                }
                else
                {
                    remindExist = vR.checkIfReminderPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);
                    
                    if(remindExist == "1")
                    {
                        //Samma, visa knapp, men inte pop-up
                        remindExist = "2";
                    }
                }
            }
            else
            {
                remindExist = vR.checkIfReminderPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);
            }

            return remindExist;
        }

        public String GetReminders()
        {
            String customer = Request.Form["customer"];
            List<view_Reminder> vR = view_Reminder.getRemindersPerCustomer(customer, System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);

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

        /// <summary>
        /// Bygg ihop Modultexterna för kontraktet
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="contractId"></param>
        /// <param name="onlyRemovedModules"></param>
        /// <returns></returns>
        private string updateDescriptions(string customer, string contractId, bool? onlyRemovedModules)
        {
            var contractRow = new view_ContractRow();
            var contractArtDescriptionList = contractRow.GetContractRowsForModuleInfo(customer, contractId, onlyRemovedModules);
            var moduleInfo = "";

            foreach (var contractArtDescription in contractArtDescriptionList)
            {
                moduleInfo += "<p>" + contractArtDescription.Contract_description + "</p>";
            }

            return moduleInfo;
        }

        /// <summary>
        /// Reading default contractdescription for an article
        /// </summary>
        /// <returns></returns>
        public string GetContractDescriptionDefaultText()
        {
            string idString = Request.Form["id"];
            string type = Request.Form["type"]; // A = Artikel, K = Konsulttjänst
            string contractDescription;
            int id;

            if (int.TryParse(idString, out id) && !string.IsNullOrEmpty(type))
            {
                if (type == "A")
                {
                    view_Module module = new view_Module();
                    module.Select("Article_number = " + id);
                    contractDescription = module.Contract_description;
                }
                else if (type == "K")
                {
                    view_Module service = new view_Module();
                    service.Select("Article_number = " + id);
                    contractDescription = service.Contract_description;
                }
                else
                {
                    return "-1";
                }
            }
            else
            {
                return "-1";
            }

            return contractDescription;
        }

    }
}