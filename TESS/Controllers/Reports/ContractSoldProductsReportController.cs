using Rotativa.MVC;
using System;
using System.Collections.Generic;
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
                "Price_category"
            };


            ViewData.Add("Printable", Printable);
            ViewData.Add("Properties", typeof(view_Module).GetProperties());
            return View();
        }

        public ActionResult Pdf(String start, String stop)
        {
            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category"
            };

            DateTime Start;
            DateTime Stop;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
           
            if (IsValidSqlDateTimeNative(start) && IsValidSqlDateTimeNative(stop))
            {
                ViewData.Add("ValidDate", true);
                Start = Convert.ToDateTime(start);
                Stop = Convert.ToDateTime(stop);
                GetFilteredModuleData(Start, Stop);
                pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \" \"";
                pdf.RotativaOptions.CustomSwitches += " --header-center \" Sold products between " + Start.ToString("yyyy'/'MM'/'dd") + " - " + Stop.ToString("yyyy'/'MM'/'dd") + " \"";
            }
            else
            {
                ViewData.Add("ValidDate", false);
            }

           
            return pdf;
 
        }

        private void GetFilteredModuleData(DateTime Start, DateTime Stop)
        {

            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category"
            };

            List<view_ContractRow> ContractRows = view_ContractRow.GetContractRowsByDateInterval(Start, Stop);
            List<view_Module> Modules = view_Module.getAllModules();

           

            List<view_Module> FilteredModules = new List<view_Module>();
            foreach (var cr in ContractRows)
            {
                foreach(var m in Modules)
                {
                    if(m.Article_number == cr.Article_number)
                    {
                        FilteredModules.Add(m);
                    }
                }
            }

            var FilteredModulesWithCount = from x in FilteredModules
                    group x by x into g
                    let count = g.Count()
                    orderby count descending
                    select new { Value = g.Key, Count = count };

            // Filter and remove duplicants
            var FilteredModules2 = Modules.Where(
               m => ContractRows.Any(
               cr => cr.Article_number == m.Article_number));

            ViewData.Add("FilteredModules", FilteredModules2);
            ViewData.Add("Printable", Printable);
            ViewData.Add("FilteredModulesWithCount", FilteredModulesWithCount);
            ViewData.Add("Properties", typeof(view_Module).GetProperties());

     
            this.ViewData["Title"] = "Contract Sold Products Report";
        }


        public String FilteredModules()
        {

            var Printable = new List<String> {
                "Article_number",
                "Module",
                "Price_category"
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
            

            List<view_ContractRow> ContractRows = view_ContractRow.GetContractRowsByDateInterval(Start, Stop);
            List<view_Module> Modules = view_Module.getAllModules();

            List<view_Module> FilteredModules = new List<view_Module>();
            foreach (var cr in ContractRows)
            {
                foreach (var m in Modules)
                {
                    if (m.Article_number == cr.Article_number)
                    {
                        FilteredModules.Add(m);
                    }
                }
            }

            var FilteredModulesWithCount = from x in FilteredModules
                                           group x by x into g
                                           let count = g.Count()
                                           orderby count descending
                                           select new { Value = g.Key, Count = count };

            // Filter and remove duplicants
            var FilteredModules2 = Modules.Where(
               m => ContractRows.Any(
               cr => cr.Article_number == m.Article_number));


            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_Module m in FilteredModules2)
            {
                Dictionary<String, String> dic = new Dictionary<String, String>();
                var LastPInfoCount = m.GetType().GetProperties().Length;
                var count = 0;
                foreach (System.Reflection.PropertyInfo pi in m.GetType().GetProperties())
                {
                    count++;

                    if (LastPInfoCount == count)
                    {
                        foreach (var dup in FilteredModulesWithCount)
                        {
                            System.Type type = dup.GetType();

                            view_Module vm = (view_Module)type.GetProperty("Value").GetValue(dup, null);
                            int dupCount = (int)type.GetProperty("Count").GetValue(dup, null);
                            if (m.Article_number == vm.Article_number)
                            {
                                dic.Add("Count", dupCount.ToString());
                            }
                        }
                    }


                    if (Printable.Contains(pi.Name))
                    {
                        if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
                        {
                            if (pi.GetValue(m) != null)
                                dic.Add(pi.Name, ((DateTime)pi.GetValue(m)).ToString("yyyy-MM-dd"));
                            else
                                dic.Add(pi.Name, null);
                        }
                        else
                        {
                            if (pi.GetValue(m) != null)
                                dic.Add(pi.Name, pi.GetValue(m).ToString());
                            else
                                dic.Add(pi.Name, null);
                        }
                    }
                }
                rows.Add(dic);
                
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(rows) + "}";
        }


    }
}