using Rotativa.MVC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class ModuleReportController : Controller
    {
        // GET: ModuleReport
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && 
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Customer Module Report";

            return View();
        }

        public ActionResult Pdf()
        {
            String articleNumbers = Request["module"];
            var articleNumbersList = (List<int>)new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];
            String exportAll = Request["exportAll"];

            var customerModulesList = new List<List<Dictionary<string, object>>>();

            var modules = new List<view_Module>();

            if(exportAll == "1") //Export ALL modules to the report...
            {
                //Get all article numbers
                articleNumbersList = view_Module.getAllModules().Select(s => (int)s.Article_number).ToList();
            }

            List<Dictionary<String, object>> list = generateModuleInfo(articleNumbersList);

            foreach (var article in articleNumbersList)
            {
                var customerModules = new List<Dictionary<string, object>>();

                view_Module module = new view_Module();
                module.Select("Article_number=" + article);
                modules.Add(module);

                foreach (var item in list)
                {
                    object moduleName = "";
                    if(item.TryGetValue("Module", out moduleName))
                    {
                        if (moduleName as string == module.Module)
                        {
                            //Match
                            customerModules.Add(item);
                        }
                    }
                }

                customerModulesList.Add(customerModules);
            }

            if (list.Count != 0)
            {
                ViewData.Add("Customermodules", customerModulesList);//(new SortedByColumnCollection(list, sortDir, sortKey)).Collection);
            }
            else
            {
                ViewData.Add("Customermodules", new ArrayList());
            }

            //this.ViewData["Title"] = module.Module;
            this.ViewData["Modules"] = modules;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + "Customer Module Report" + "\"" + " --header-spacing \"3\"";


            return pdf;
        }

        public List<Dictionary<String, object>> generateModuleInfo(List<int> articleNumbers)
        {

            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            if(articleNumbers != null)
            {
                foreach (var articleNumber in articleNumbers)
                {
                    List<view_ContractRow> ContractRows = view_ContractRow.GetValidContractRows(articleNumber);
                    List<view_Contract> contracts = new List<view_Contract>();
                    view_Module module = new view_Module();
                    module.Select("Article_number=" + articleNumber);

                    if (System.Web.HttpContext.Current.GetUser().IfSameArea(module.Area))
                    {
                        foreach (view_ContractRow cr in ContractRows)
                        {
                            Dictionary<String, object> Customers = new Dictionary<String, object>();
                            if (contracts.FindIndex(m => m.Contract_id == cr.Contract_id) <= 0)
                            {
                                view_Contract c = new view_Contract("Contract_id = '" + cr.Contract_id + "'");
                                contracts.Add(c);
                            }
                            view_Contract contract = contracts[contracts.Count - 1];
                            if (contract.Status == "Giltigt")
                            {
                                Customers.Add("Customer", cr.Customer);
                                Customers.Add("Contract_id", cr.Contract_id);
                                Customers.Add("Module", module.Module);
                                Customers.Add("Representative", contract.Sign);
                                Customers.Add("Classification", module.Classification);

                                rows.Add(Customers);
                            }
                        }
                    }
                }
            }

            return rows;
        }

        public String Module()
        {
            try
            {
                String articleNumbers = Request.Form["module"];
                var articleNumbersDic = (List<int>) new JavaScriptSerializer().Deserialize(articleNumbers, typeof(List<int>));

                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(generateModuleInfo(articleNumbersDic)) + "}";
            }
            catch(Exception ex)
            {
                return "0";
            }
        }
        public string ExportExcel()
        {
            String exportAll = Request["exportAll"];
            var articleNumbers = Request["module"];

            if (exportAll == "1") //Export ALL modules to the report...
            {
                //Get all article numbers
                var articleNumbersList = view_Module.getAllModules().Select(s => (int)s.Article_number).ToList();

                foreach (var number in articleNumbersList)
                {
                    articleNumbers += number + ",";
                }

                articleNumbers = articleNumbers.Substring(0, articleNumbers.Length - 1);
            }

            System.Data.DataTable dt = view_ContractRow.ExportValidContractRowsToExcel(articleNumbers, exportAll != null ? true : false);
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            return ex.Export(dt, "ModuleReport.xlsx");
        }
    }
}