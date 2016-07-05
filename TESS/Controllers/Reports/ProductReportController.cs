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
    public class ProductReportController : Controller
    {
        public ActionResult Index()
        {
            ViewData.Add("Title", "Product Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, String>> modules = this.GenerateProducts();
            ViewData.Add("Modules", modules);

            this.ViewData["Title"] = "Product Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Product Report\"";

            return pdf;


        }

        public String Products()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateProducts()) + "}";
        }

        public List<Dictionary<String, String>> GenerateProducts()
        {
            List<view_Module> modules = view_Module.getAllModules();
            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_Module module in modules)
            {
                if(module.Expired != null && module.Expired == false)
                {
                    Dictionary<String, String> dict = new Dictionary<String, String>();

                    dict.Add("article_number", module.Article_number.ToString());
                    dict.Add("name", module.Module);
                    dict.Add("system", module.System);
                    dict.Add("classification", module.Classification);

                    rows.Add(dict);
                }
            }
            return rows;
        }
    }
}