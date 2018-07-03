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
            modules = modules.Where(m => m.Discount_type == 0 && m.Expired == false && 
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Customer Module Report";

            return View();
        }

        public ActionResult Pdf()
        {
            String aNumb = Request["module"];
            List<Dictionary<String, object>> list = generateModuleInfo(aNumb);

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];
            if(list.Count != 0){
                ViewData.Add("Customermodules", (new SortedByColumnCollection(list, sortDir, sortKey)).Collection);
            }
            else
            {
                ViewData.Add("Customermodules", new ArrayList());
            }
            

            view_Module module = new view_Module();
            module.Select("Article_number=" + aNumb);

            this.ViewData["Title"] = module.Module;
            this.ViewData["Module"] = module;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + "Customer Module Report" + "\"";
            

            return pdf;


        }

        public List<Dictionary<String, object>> generateModuleInfo(String articleNumber)
        {
            List<view_ContractRow> ContractRows = view_ContractRow.GetValidContractRows(int.Parse(articleNumber));
            List<view_Contract> contracts = new List<view_Contract>();
            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            view_Module module = new view_Module();
            module.Select("Article_number=" + articleNumber);

            if (System.Web.HttpContext.Current.GetUser().IfSameArea(module.Area))
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
                        Customers.Add("Representative", contract.Sign);
                        Customers.Add("Classification", module.Classification);
                        
                        rows.Add(Customers);
                    }
                }
            return rows;
        }

        public String Module()
        {
            String articleNumber = Request.Form["module"];

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(generateModuleInfo(articleNumber)) + "}";
        }
        public string ExportExcel()
        {
            System.Data.DataTable dt = view_ContractRow.ExportValidContractRowsToExcel(Request["module"]);
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            return ex.Export(dt, "ModuleReport.xlsx");
        }
    }
}