﻿using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Extensions;


namespace TietoCRM.Controllers
{
    public static class KFJas
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

    public class CustomerOfferController : Controller
    {
        // GET: CustomerOffer
        public ActionResult Index()
        {
            this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            this.ViewData.Add("ControllerName", "CustomerOffer");
            this.ViewData.Add("AjaxUrl", "/CustomerOffer/CustomerOfferJsonData/");
            this.ViewData.Add("TargetUrl", "/CustomerOffer/Data/");
            this.ViewData.Add("InsertUrl", "/CustomerOffer/Insert/");
            this.ViewData.Add("DeleteUrl", "/CustomerOffer/Delete/");
            this.ViewData.Add("PrimaryKey", "SSMA_timestamp");
            this.ViewData["title"] = "Customer Offer";

            String on;
            if (ViewBag.Customers.Count <= 0)
                on = "";
            else
                on = ViewBag.Customers[0];

            if (Request["selected-offer"] != null && Request["selected-offer"] != "")
            {
                int number = Convert.ToInt32(Request["selected-offer"]);
                view_CustomerOffer co = new view_CustomerOffer("Offer_number = " + number);
                on = co.Customer;
            }

            this.ViewData.Add("OfferNumber", on);

            List<String> columnNames = new List<String>();
            columnNames.Add("#");
            columnNames.Add("Offer_number");
            columnNames.Add("Customer");
            columnNames.Add("Contact_person");
            columnNames.Add("Email");
            columnNames.Add("Offer_created");
            columnNames.Add("Offer_valid");
            columnNames.Add("Offer_status");
            this.ViewData.Add("Properties", columnNames);
            List<String> offerStatus = new List<String>();
            offerStatus = GetOfferStatus();
            ViewData.Add("OfferStatus", offerStatus);

            List<List<String>> selectOfferStatus = new List<List<String>>();
            foreach (String status in offerStatus)
            {
                List<String> selectItem = new List<String>();
                selectItem.Add(status);
                selectItem.Add(status);

                selectOfferStatus.Add(selectItem);
            }
            ViewData.Add("SelectOfferStatus", selectOfferStatus);

            return View();
        }

        public ActionResult ViewPdf()
        {
            string request = Request["selected-offer"];
            int offerID = 0;
            if (!String.IsNullOrEmpty(request))
            {
                offerID = int.Parse(request);
            }
            view_CustomerOffer co = new view_CustomerOffer("Offer_number = " + offerID);
            ViewData.Add("CustomerOffer", co);

            List<dynamic> articles = new List<dynamic>();
            List<dynamic> educationPortals = new List<dynamic>();

            foreach (view_OfferRow offerRow in co._OfferRows)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + offerRow.Article_number);
                dynamic offerInfo = new ExpandoObject();
                offerInfo.Article_number = module.Article_number;
                offerInfo.Module = module.Module;
                offerInfo.System = module.System;
                offerInfo.Classification = module.Classification;
                offerInfo.License = offerRow.License;
                offerInfo.Maintenance = offerRow.Maintenance;
                offerInfo.SortNr = offerRow.Sortnr;

                if (module.System != "Lärportal")
                {
                    articles.Add(offerInfo);
                }
                else
                {
                    educationPortals.Add(offerInfo);
                }
            }

            articles = articles.OrderBy(a => a.SortNr).ThenBy(a => a.Article_number).ToList();
            educationPortals = educationPortals.OrderBy(a => a.Article_number).ToList();

            ViewData.Add("EducationPortals", educationPortals);
            ViewData.Add("Articles", articles);

            view_CustomerContact cc = new view_CustomerContact();
            cc.Select("Customer = '" + co.Customer + "' AND Contact_person = '" + co.Contact_person + "'");
            ViewData.Add("CustomerContact", cc);

            view_Customer customer = new view_Customer();
            customer.Select("Customer = '" + co.Customer.ToString() + "'");
            ViewData.Add("Customer", customer);

            view_User user = new view_User();
            user.Select("Sign = '" + customer.Representative + "'");
            ViewData.Add("Representative", user);

            this.ViewData["Title"] = "Customer Offer";

            string offerRequest = Request["offer-section"];
            if (!String.IsNullOrEmpty(offerRequest))
            {
                if (offerRequest == "_OfferHTML_OfferSection")
                    return View(offerRequest);
                else
                    return View();
            }
            else
                return View();

        }

        public ActionResult Pdf()
        {
            string request = Request["selected-offer"];
            int offerID = 1;
            if (!String.IsNullOrEmpty(request))
            {
                offerID = int.Parse(request);
            }
            view_CustomerOffer co = new view_CustomerOffer("Offer_number = " + offerID);
            ViewData.Add("CustomerOffer", co);

            List<dynamic> articles = new List<dynamic>();
            List<dynamic> educationPortals = new List<dynamic>();

            foreach (view_OfferRow offerRow in co._OfferRows)
            {
                view_Module module = new view_Module();
                module.Select("Article_number = " + offerRow.Article_number);
                dynamic offerInfo = new ExpandoObject();
                offerInfo.Article_number = module.Article_number;
                offerInfo.Module = module.Module;
                offerInfo.System = module.System;
                offerInfo.Classification = module.Classification;
                offerInfo.License = offerRow.License;
                offerInfo.Maintenance = offerRow.Maintenance;
                offerInfo.SortNr = offerRow.Sortnr;

                if (module.System != "Lärportal")
                {
                    articles.Add(offerInfo);
                }
                else
                {
                    educationPortals.Add(offerInfo);
                }
            }

            articles = articles.OrderBy(a => a.SortNr).ThenBy(a => a.Article_number).ToList();
            educationPortals = educationPortals.OrderBy(a => a.Article_number).ToList();

            ViewData.Add("EducationPortals", educationPortals);
            ViewData.Add("Articles", articles);

            view_CustomerContact cc = new view_CustomerContact();
            cc.Select("Customer = '" + co.Customer + "' AND Contact_person = '" + co.Contact_person + "'");
            ViewData.Add("CustomerContact", cc);

            view_Customer customer = new view_Customer();
            customer.Select("Customer = '" + co.Customer + "'");
            ViewData.Add("Customer", customer);

            view_User user = new view_User();
            user.Select("Sign = '" + customer.Representative + "'");
            ViewData.Add("Representative", user);
            String footerFilePath = "file:///" + Server.MapPath("~/Views/Shared/Footer.html").Replace("\\", "/");
            String headerFilePath = "file:///" + Server.MapPath("~/Views/CustomerOffer/Header.html").Replace("\\", "/");
            string cusomtSwitches = string.Format("--print-media-type --header-spacing 4 --header-html \"{1}\" --footer-html \"{0}\" ", footerFilePath, headerFilePath);
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = cusomtSwitches;

            // Set title and file names.
            String fileName = customer.Customer + " " + request + ".pdf";
            fileName = fileName.Replace(" ", "_");
            ViewData["Title"] = fileName;
            Response.Headers["Content-disposition"] = "inline; filename=" + fileName;

            return pdf;
        }

        public ActionResult Footer()
        {
            return View();
        }
        public ActionResult Modals()
        {
            this.ViewData.Add("Services", view_Service.getAllServices());

            string request = Request["selected-offer"];

            int offerID = 1;
            if (!String.IsNullOrEmpty(request))
            {
                offerID = int.Parse(request);
            }

            string sign = "";
            List<view_User> userList = view_User.getAllUsers();
            if (userList.Any(user => user.Sign == Request["sign"]))
                sign = Request["sign"];
            else
            {
                return new EmptyResult();
            }

            String customer = Server.UrlDecode(Request["customer"]);
            ViewData.Add("CustomerName", customer);


            ViewData.Add("Systems", GetAllSystemNames());

            ViewData.Add("Statuses", GetOfferStatus());

            view_CustomerOffer co = new view_CustomerOffer("Offer_number = " + offerID);
            ViewData.Add("CustomerOffer", co);

            ViewData.Add("ContactPersons", view_CustomerContact.getAllCustomerContacts(customer));

            ViewData.Add("CustomerOfferJSON", (new JavaScriptSerializer()).Serialize(co));

            List<view_TextTemplate> tts = view_TextTemplate.getTextTemplatesType("Offert", sign);
            ViewData.Add("TextTemplates", tts);
            return View();
        }

        public String GetCustomerContact()
        {
            int offerID = int.Parse(Request.Form["offerNumber"]);
            String customer = Request.Form["customer"];
            view_CustomerOffer co = new view_CustomerOffer("Offer_number = " + offerID);
            List<view_CustomerContact> l = view_CustomerContact.getAllCustomerContacts(customer);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            foreach (view_CustomerContact cc in l)
            {
                if (cc.Contact_person == co.Contact_person)
                    return jss.Serialize(cc);
            }
            return "[]";
        }

        public String CustomerOffer()
        {
            String customer = Request.Form["customer"];

            return (new JavaScriptSerializer()).Serialize(view_CustomerOffer.getAllCustomerOffers(customer));
        }

        public String CustomerOfferJsonData()
        {
            String customer = Request.Form["customer"];

            List<String> customerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            if (customer == "" || customer == null)
            {
                if (customerNames.Count <= 0)
                    customer = "-1337_ingen-kund.pådenna#sökning?!"; // a string that will make sure we wont get any result. having an empty string gave result, because it exists offers with empty strings as customers
                else
                    customer = customerNames[0];
            }

            List<view_CustomerOffer> customerOffers = view_CustomerOffer.getAllCustomerOffers(customer);
            List<dynamic> customers = new List<dynamic>();

            foreach (view_CustomerOffer co in customerOffers)
            {
                view_CustomerContact cc = new view_CustomerContact();
                cc.Select("Customer = '" + co.Customer + "' AND Contact_person = '" + co.Contact_person + "'");
                var v = new
                {
                    Offer_number = co._Offer_number,
                    Customer = co.Customer,
                    Contact_person = co.Contact_person,
                    Email = cc.Email,
                    Offer_created = co.Offer_created,
                    Offer_valid = co.Offer_valid,
                    Offer_status = co.Offer_status,
                    SSMA_timestamp = co.SSMA_timestamp
                };

                customers.Add(v);
            }

            //ViewData.Add("viewRemind", checkReminder(customer));

            this.Response.ContentType = "text/plain";
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(customers) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
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

        public String JsonData()
        {
            String requestData = Request.Form["requestData"];

            if (requestData == "view_TextTemplate")
            {
                int templateId = int.Parse(Request.Form["templateId"]);

                view_TextTemplate textTemplate = new view_TextTemplate();
                textTemplate.Select("ID_PK = " + templateId);

                return (new JavaScriptSerializer()).Serialize(textTemplate);
            }
            else if (requestData == "update_view_CustomerOffer")
            {
                return this.Data("Offer_number");
            }
            else if (requestData == "update_head_information")
            {
                try
                {
                    // Customer to select from DB. This might change after update.
                    String offerNumber = Request.Form["offerNumber"];
                    String objectData = Request.Form["object"];


                    Dictionary<String, Object> offerVariables = null;

                    try
                    {
                        offerVariables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(objectData, typeof(Dictionary<String, dynamic>));
                    }
                    catch
                    {
                        return "0";
                    }

                    try
                    {
                        view_CustomerOffer co = new view_CustomerOffer();
                        co.Select("Offer_number = '" + offerNumber + "'");

                        foreach (KeyValuePair<String, object> offerVariable in offerVariables)
                        {
                            co.SetValue(offerVariable.Key, offerVariable.Value);
                        }

                        co.Update("Offer_number = '" + offerNumber + "'");
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        // Return -1 on any possible arror
                        return "-1";
                    }


                    return "1";
                }
                catch
                {
                    return "-1";
                }
            }
            else if (requestData == "update_classification_select")
            {
                String System = Request.Form["System"];
                List<String> classificationList = new List<String>();
                String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    String queryTextClassification = @"SELECT Procapita, Indelning FROM V_Procapita WHERE Procapita = @Procapita";
                    command.CommandText = queryTextClassification;
                    command.Prepare();

                    command.Parameters.AddWithValue("@Procapita", System);
                    command.ExecuteNonQuery();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            classificationList.Add(reader["indelning"].ToString());
                        }
                    }
                }
                string temp = classificationList[0];
                classificationList.Remove(temp);
                classificationList.Add(temp);

                return (new JavaScriptSerializer()).Serialize(classificationList);
            }
            else if (requestData == "update_view_Service")
            {
                String obj = Request.Form["object"];
                int offer = Convert.ToInt32(Request.Form["offer"]);

                List<dynamic> list = null;
                try
                {
                    list = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(obj, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                view_CustomerOffer customerOffer = new view_CustomerOffer("Offer_number = " + offer);
                // remove all consultant row to later insert the new ones
                foreach (view_ConsultantRow cr in customerOffer._ConsultantRows)
                {
                    cr.Delete("Offer_number = " + cr.Offer_number + " AND Code = " + cr.Code);
                }

                foreach (Dictionary<String, Object> dict in list)
                {
                    int id = Convert.ToInt32(dict["id"]);
                    int amount = Convert.ToInt32(dict["amount"]);
                    int total = Convert.ToInt32(dict["total"]);

                    view_ConsultantRow consultantRow = new view_ConsultantRow();
                    consultantRow.Offer_number = offer;
                    consultantRow.Code = id;
                    consultantRow.Amount = amount;
                    consultantRow.Total_price = total;
                    consultantRow.Include_status = false;
                    consultantRow.Insert();
                }

                return "1";

            }
            #region GET_MODULES
            else if (requestData == "get_modules")
            {
                String customer = Request.Form["customer"];
                String System = Request.Form["System"];
                String classification = Request.Form["classification"];

                String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    String queryText = @"SELECT view_Module.Article_number, view_Module.Module, view_Tariff.License, view_Tariff.Maintenance, 
                                        view_Module.Price_category, view_Module.System, view_Module.Classification, view_Module.Comment
                                        FROM view_Module                                                                                       
                                        JOIN view_Tariff                                                                                       
                                        on view_Module.Price_category = view_Tariff.Price_category
                                        WHERE System = @System AND Classification = @classification AND Expired = 0
                                        AND Inhabitant_level = (
                                            Select ISNULL(Inhabitant_level, 1) AS I_level from view_Customer
                                            where Customer = @customer
                                        )
                                        order by Article_number asc";

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

                    List<view_ContractRow> rows = view_ContractRow.GetAllContractRows(customer).DistinctBy(cr => cr.Article_number).ToList();

                    foreach (Dictionary<String, dynamic> kv in resultList)
                    {
                        if (rows.Any(cr => cr.Article_number == kv["Article_number"]))
                            kv.Add("Used", true);
                    }
                    //Response.Charset = "UTF-8";
                    // this.solve();
                    String resultString = (new JavaScriptSerializer()).Serialize(resultList);
                    return resultString;

                }
            }
            #endregion
            #region GET_MODULES_ALL
            else if (requestData == "get_modules_all")
            {
                String customer = Request.Form["customer"];
                String searchtext = Request.Form["searchtext"];

                String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    String queryText = @"SELECT view_Module.Article_number, view_Module.Module, view_Tariff.License, view_Tariff.Maintenance,
                                        view_Module.Price_category, view_Module.System, view_Module.Classification, view_Module.Comment
                                        FROM view_Module                                                                                       
                                        JOIN view_Tariff                                                                                       
                                        on view_Module.Price_category = view_Tariff.Price_category
                                        WHERE Expired = 0 And (Cast(view_Module.Article_number As Varchar(30)) Like Case @searchtext When '' Then Cast(view_Module.Article_number As Varchar(30)) Else @searchtext End Or
                                        view_Module.Module Like Case @searchtext When '' Then view_Module.Module Else @searchtext End)
                                        AND Inhabitant_level = (
                                            Select ISNULL(Inhabitant_level, 1) AS I_level from view_Customer
                                            where Customer = @customer
                                        )
                                        order by Article_number asc";

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
                        if (rows.Any(cr => cr.Article_number == kv["Article_number"]))
                            kv.Add("Used", true);
                    }
                    //Response.Charset = "UTF-8";
                    // this.solve();
                    String resultString = (new JavaScriptSerializer()).Serialize(resultList);
                    return resultString;

                }
            }
            #endregion
            else if (requestData == "update_module_rows")
            {
                String selectedArticles = Request.Form["Object"];
                int Offer_number = Convert.ToInt32(Request.Form["Offer_number"]);

                List<dynamic> list = null;
                try
                {
                    list = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(selectedArticles, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                view_CustomerOffer customerOffer = new view_CustomerOffer("Offer_number = " + Offer_number);
                // remove all consultant row to later insert the new ones
                foreach (view_OfferRow or in customerOffer._OfferRows)
                {
                    or.Delete("Offer_number = " + or.Offer_number + " AND Article_number = " + or.Article_number);
                }

                foreach (Dictionary<String, Object> dict in list)
                {
                    int Article_number = Convert.ToInt32(dict["Article_number"]);
                    Decimal License = 0;
                    if(dict.Keys.Contains("License"))
                        License = Decimal.Parse(dict["License"].ToString().Replace(",", "."), NumberFormatInfo.InvariantInfo);
                    Decimal Maintenance = Decimal.Parse(dict["Maintenance"].ToString().Replace(",","."), NumberFormatInfo.InvariantInfo);

                    view_OfferRow offerRow = new view_OfferRow();
                    offerRow.Offer_number = Offer_number;
                    offerRow.Article_number = Article_number;
                    offerRow.License = Convert.ToDecimal(License);
                    offerRow.Maintenance = Convert.ToDecimal(Maintenance);
                    offerRow.Include_status = false;
                    offerRow.Insert();
                }

                return "1";

            }
            else
                return "";
        }

        [ValidateInput(false)]
        public String Data(String key = "SSMA_timestamp")
        {

            try
            {

                //String json = Request.Form["object"];
                String json = HttpContext.Request.Unvalidated.Form["object"];
                List<dynamic> map = null;
                try
                {
                    map = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }
                if (map.Count == 0)
                {
                    return "0";
                }
                foreach (Dictionary<String, object> d in map)
                {
                    SQLBaseClass a = new view_CustomerOffer();
                    if (d.ContainsKey("insert"))
                    {
                        foreach (KeyValuePair<String, object> entry in d)
                        {
                            if (entry.Key != "insert")
                                a.SetValue(entry.Key, entry.Value);
                        }
                        a.Insert();
                    }
                    else
                    {
                        String compareValue = Convert.ToString(d["primaryKey"]);
                        a.Select(key + " = " + compareValue);
                        foreach (KeyValuePair<String, object> entry in d)
                        {
                            a.SetValue(entry.Key, entry.Value);
                        }
                        a.Update(key + " = " + compareValue);
                    }
                }
                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
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

                foreach (view_Contract contract in view_Contract.GetContracts(compareValue1))
                {
                    if (contract.Contact_person == compareValue2)
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

        public String InsertContact()
        {
            try
            {
                String json = Request.Form["json"];
                view_CustomerContact a = null;
                try
                {
                    a = (view_CustomerContact)(new JavaScriptSerializer()).Deserialize(json, typeof(view_CustomerContact));
                }
                catch (Exception e)
                {
                    return "0";
                }

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String Insert()
        {
            try
            {
                String json = Request.Form["json"];
                view_CustomerOffer a = null;
                try
                {
                    a = (view_CustomerOffer)(new JavaScriptSerializer()).Deserialize(json, typeof(view_CustomerOffer));
                }
                catch (Exception e)
                {
                    return "0";
                }

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String Delete()
        {
            try
            {
                String value = Request.Form["primaryKey"];
                view_Tariff a = new view_Tariff();
                //a.Select("Article_number = " + value);
                a.Delete("SSMA_timestamp = '" + value + "'");
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
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

        public List<String> GetOfferStatus()
        {
            List<String> offerStatus = new List<string>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Status FROM view_OfferStatus";

                command.Prepare();



                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        offerStatus.Add(reader["Status"].ToString());
                    }
                }


            }
            return offerStatus;
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