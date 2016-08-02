using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class ContractSoldProductsReportController : Controller
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
            DateTime testDate = DateTime.MinValue;
            System.Data.SqlTypes.SqlDateTime sdt;
            if (DateTime.TryParse(someval, out testDate))
            {
                try
                {
                    // take advantage of the native conversion
                    sdt = new System.Data.SqlTypes.SqlDateTime(testDate);
                    valid = true;
                }
                catch (System.Data.SqlTypes.SqlTypeException ex)
                {

                    // no need to do anything, this is the expected out of range error
                }
            }

            return valid;
        }


        // GET: ContractSoldProducts
        public ActionResult Index(String start, String stop)
        {
            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category",
                "System",
                "Classification"
            };


            ViewData.Add("Printable", Printable);
            ViewData.Add("Properties", typeof(view_Module).GetProperties());
            ViewData.Add("Title", "Sold Products Report");
            return View();
        }

        public ActionResult Pdf(String start, String stop)
        {
            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category",
                "System",
                "Classification"
            };

            DateTime Start;
            DateTime Stop;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
           
            if (IsValidSqlDateTimeNative(start) && IsValidSqlDateTimeNative(stop))
            {
                ViewData.Add("ValidDate", true);
                Start = Convert.ToDateTime(start);
                Stop = Convert.ToDateTime(stop);
                String sortDir = Request["sort"];
                String sortKey = Request["prop"];

                var sortedList = GetFilteredModules(Start, Stop).ToList();
 
                ViewData.Add("ReturnModules", (new SortedByColumnCollection(sortedList, sortDir, sortKey).Collection));
                ViewData.Add("Printable", Printable);
                ViewData.Add("Properties", typeof(view_Module).GetProperties());
                pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \" \"";
                pdf.RotativaOptions.CustomSwitches += " --header-center \" Sold products between " + Start.ToString("yyyy'/'MM'/'dd") + " - " + Stop.ToString("yyyy'/'MM'/'dd") + " \"";
            }
            else
            {
                ViewData.Add("ValidDate", false);
            }

           
            return pdf;
 
        }

        private List<Dictionary<String, object>> GetFilteredModules(DateTime Start, DateTime Stop)
        {
            

            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category",
                "System",
                "Classification"
            };

            List<view_ContractRow> ContractRows = view_ContractRow.GetContractRowsByDateInterval(Start, Stop);
            List<view_Module> Modules = view_Module.getAllModules();

            Dictionary<int, Dictionary<String, object>> ReturnModules = new Dictionary<int, Dictionary<String, object>>();
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
                       
                    }
                }
            }
            return ReturnModules.Values.ToList();

           
        }


        public String FilteredModules()
        {

            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category",
                "Classification"
            };
            String startRe;
            String stopRe;
            DateTime Start;
            DateTime Stop;
            try
            {
                if(IsValidSqlDateTimeNative(Request.Form["start"]) && IsValidSqlDateTimeNative(Request.Form["stop"]))
                {
                    startRe = Request.Form["start"];
                    stopRe = Request.Form["stop"];
                    Start = Convert.ToDateTime(startRe);
                    Stop = Convert.ToDateTime(stopRe);
                }
                else
                {
                    return "{\"data\": {}}";
                }
            }
            catch(Exception ignore)
            {
                return "{\"data\": {}}";
            }
            

            
           

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GetFilteredModules(Start, Stop)) + "}";
        }


    }
}