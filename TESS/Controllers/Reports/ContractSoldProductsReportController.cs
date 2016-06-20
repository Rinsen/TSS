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

                var sortedList = GetFilteredModules(Start, Stop).ToList();
                sortedList.Sort((pair1, pair2) => Convert.ToInt32(pair2.Value["Count"]).CompareTo(Convert.ToInt32(pair1.Value["Count"])));

                ViewData.Add("ReturnModules", sortedList);
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

        private Dictionary<int, Dictionary<String, dynamic>> GetFilteredModules(DateTime Start, DateTime Stop)
        {
            CultureInfo se = CultureInfo.CreateSpecificCulture("sv-SE");

            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category",
                "Classification"
            };

            List<view_ContractRow> ContractRows = view_ContractRow.GetContractRowsByDateInterval(Start, Stop);
            List<view_Module> Modules = view_Module.getAllModules();

            Dictionary<int, Dictionary<String, dynamic>> ReturnModules = new Dictionary<int, Dictionary<String, dynamic>>();
           
            foreach (var cr in ContractRows)
            {
                foreach(var m in Modules)
                {
                    if(m.Article_number == cr.Article_number)
                    {
                        Dictionary<String, dynamic> SortedModule = new Dictionary<String, dynamic>();
                        SortedModule.Add("Count", 1);
                      
                        foreach(System.Reflection.PropertyInfo pi in m.GetType().GetProperties())
                        {
                            if(pi.Name == "Price_category")
                                SortedModule.Add(pi.Name, String.Format(se, "{0:C0}", pi.GetValue(m)).Replace(".", " ").Replace(" kr", ""));
                            else
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

            return ReturnModules;

            //ViewData.Add("ReturnModules", ReturnModules);
            //ViewData.Add("Printable", Printable);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());


            //this.ViewData["Title"] = "Contract Sold Products Report";
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
            

            
            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (KeyValuePair<int, Dictionary<String, dynamic>> FM in this.GetFilteredModules(Start,Stop))
            {
                Dictionary<String, String> dic = new Dictionary<String, String>();
                foreach (KeyValuePair<String,dynamic> FMS in FM.Value)
                {
                    if(FMS.Value != null)
                    {
                        dic.Add(FMS.Key, FMS.Value.ToString());
                    }
                    else
                    {
                        dic.Add(FMS.Key, "");
                    }
                    
                }
                rows.Add(dic);   
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(rows) + "}";
        }


    }
}