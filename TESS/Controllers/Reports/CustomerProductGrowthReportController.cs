using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class CustomerProductGrowthReportController : Controller
    {
        /// <summary>
        /// An better method to verify whether a value is 
        /// kosher for SQL Server datetime. This uses the native library
        /// for checking range values
        /// </summary>
        /// <param name="someval">A date string that may parse</param>
        /// <returns>true if the parameter is valid for SQL Sever datetime</returns>
        static bool IsValidSqlDateTimeNative(string someval)
        {
            bool valid = false;
            DateTime testDate;
            System.Data.SqlTypes.SqlDateTime sdt;
            if (DateTime.TryParse(someval, out testDate))
            {
                try
                {
                    // take advantage of the native conversion
                    sdt = new System.Data.SqlTypes.SqlDateTime(testDate);
                    valid = true;
                }
                catch (System.Data.SqlTypes.SqlTypeException)
                {
                    // no need to do anything, this is the expected out of range error
                }
            }

            return valid;
        }


        // GET: ContractSoldProducts
        public ActionResult Index(string start, string stop)
        {
            var Printable = new List<string> {
                "Article_number",
                "Module",
                "Price_category",
                "System",
                "Classification"
            };

            ViewData.Add("Printable", Printable);
            ViewData.Add("Properties", typeof(view_Module).GetProperties());
            ViewData.Add("Customers", view_Customer.getAllCustomers());

            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 &&
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);

            ViewData.Add("Title", "Customer Product Growth Report");

            return View();
        }

        public ActionResult Pdf(string start, string stop, string customers, string modules)
        {
            var Printable = new List<string> {
                "Contract_id",
                "Customer",
                "Article_number",
                "License",
                "Maintenance",
                "Rewritten",
                "New",
                "Removed"
            };

            DateTime Start;
            DateTime Stop;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
           
            if (IsValidSqlDateTimeNative(start) && IsValidSqlDateTimeNative(stop))
            {
                ViewData.Add("ValidDate", true);
                Start = Convert.ToDateTime(start);
                Stop = Convert.ToDateTime(stop);
                string sortDir = Request["sort"];
                string sortKey = Request["prop"];
                var customersDic = new List<string>();
                var articleNumbersDic = new List<int>();

                try
                {
                    if(!string.IsNullOrEmpty(customers))
                    {
                        customersDic = customers.Split(',').ToList();
                    }
                    if (!string.IsNullOrEmpty(modules))
                    {
                        articleNumbersDic = modules.Split(',').Select(int.Parse).ToList();
                    }

                    var sortedList = view_ContractRow.GetContractRowsByDateIntervalCustomersAndArticleNumbers(Start, Stop, customersDic, articleNumbersDic);

                    ViewData.Add("ReturnRows", sortedList);
                    ViewData.Add("Printable", Printable);
                    ViewData.Add("Properties", typeof(view_ContractRow).GetProperties());

                    pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \" \"";
                    pdf.RotativaOptions.CustomSwitches += " --header-center \" Sold products between " + Start.ToString("yyyy'/'MM'/'dd") + " - " + Stop.ToString("yyyy'/'MM'/'dd") + " \"";
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            else
            {
                ViewData.Add("ValidDate", false);
            }

            return pdf; 
        }

        private List<Dictionary<string, object>> GetFilteredModules(DateTime Start, DateTime Stop, List<string> customers, List<int> articleNumbers)
        {
            //var Printable = new List<string> {
            //    "Article_number",
            //    "Module",
            //    "Price_category",
            //    "System",
            //    "Classification"
            //};

            List<view_ContractRow> ContractRows = view_ContractRow.GetContractRowsByDateIntervalCustomersAndArticleNumbers(Start, Stop, customers, articleNumbers);
            List<view_Module> Modules = view_Module.getAllModules();

            Dictionary<int, Dictionary<string, object>> ReturnModules = new Dictionary<int, Dictionary<String, object>>();
            foreach (view_ContractRow cr in ContractRows)
            {
                foreach(view_Module m in Modules)
                {
                    if(m.Article_number == cr.Article_number && System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area))
                    {
                        Dictionary<String, object> SortedModule = new Dictionary<String, object>();

                        SortedModule.Add("Count", 1);
                      
                        foreach(System.Reflection.PropertyInfo pi in m.GetType().GetProperties())
                        {
                            SortedModule.Add(pi.Name, pi.GetValue(m));
                        }

                        int CurrentKey = Convert.ToInt32(m.Article_number);

                        if (ReturnModules.ContainsKey(CurrentKey))
                        {
                            int newCount = Convert.ToInt32(ReturnModules[CurrentKey]["Count"]);                        
                            ReturnModules[CurrentKey]["Count"] = newCount + 1;    
                        }
                        else
                        {
                            ReturnModules.Add(Convert.ToInt32(m.Article_number),SortedModule);
                        }

                        break; //When we found it, we found it. Go to next contract row...
                    }
                }
            }

            return ReturnModules.Values.ToList();           
        }


        public string FilteredModules()
        {

            //var Printable = new List<string> {
            //    "Article_number",
            //    "Module",
            //    "Price_category",
            //    "Classification"
            //};

            string startRe;
            string stopRe;
            DateTime Start;
            DateTime Stop;
            List<string> customersDic;
            List<int> articleNumbersDic;

            try
            {
                if(IsValidSqlDateTimeNative(Request.Form["start"]) && IsValidSqlDateTimeNative(Request.Form["stop"]))
                {
                    startRe = Request.Form["start"];
                    stopRe = Request.Form["stop"];
                    Start = Convert.ToDateTime(startRe);
                    Stop = Convert.ToDateTime(stopRe);

                    var customers = Request.Form["customers"];
                    var articleNumbers = Request.Form["modules"];

                    customersDic = (List<string>)new JavaScriptSerializer().Deserialize(customers, typeof(List<string>));
                    articleNumbersDic = (List<int>)new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));
                }
                else
                {
                    return "{\"data\": {}}";
                }
            }
            catch(Exception)
            {
                return "{\"data\": {}}";
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(GetFilteredModules(Start, Stop, customersDic, articleNumbersDic)) + "}";
        }

        public string ExportExcel(string start, string stop, string customers, string articleNumbers)
        {
            DateTime Start;
            DateTime Stop;
            var customersDic = new List<string>();
            var articleNumbersDic = new List<int>();

            try
            {
                Start = Convert.ToDateTime(start);
                Stop = Convert.ToDateTime(stop);

                if (!string.IsNullOrEmpty(customers))
                {
                    customersDic = customers.Split(',').ToList();
                }
                if (!string.IsNullOrEmpty(articleNumbers))
                {
                    articleNumbersDic = articleNumbers.Split(',').Select(int.Parse).ToList();
                }
            }
            catch (Exception)
            {
                return "-1";
            }

            System.Data.DataTable dt = view_ContractRow.ExportContractRowsByCustomerArticleAndDateIntervalToExcel(Start, Stop, customersDic, articleNumbersDic);
            
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            
            return ex.Export(dt, "CustomerProductGrowthReport.xlsx");
        }
    }
}