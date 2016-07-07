using Rotativa.MVC;
using System;
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
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList<view_Module>();

            ViewData.Add("Modules", modules);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());

            this.ViewData["Title"] = "Module Report";

            return View();
        }

        public ActionResult Pdf()
        {
            String aNumb = Request["module"];
            List<Dictionary<String, String>> list = generateModuleInfo(aNumb);

            ViewData.Add("Customermodules", list);

            view_Module module = new view_Module();
            module.Select("Article_number=" + aNumb);

            this.ViewData["Title"] = module.Module;

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + "Module Report" + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \""+ module.Module + "\"";

            return pdf;


        }

        public List<Dictionary<String,String>> generateModuleInfo(String articleNumber)
        {
            List<view_ContractRow> ContractRows = view_ContractRow.GetValidContractRows(int.Parse(articleNumber));
            List<view_Contract> contracts = new List<view_Contract>();
            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            view_Module module = new view_Module();
            module.Select("Article_number=" + articleNumber);

            if (System.Web.HttpContext.Current.GetUser().IfSameArea(module.Area))
                foreach (view_ContractRow cr in ContractRows)
                {
                    Dictionary<String, String> Customers = new Dictionary<String, String>();
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
    }
}