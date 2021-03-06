﻿using Rotativa.MVC;
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
    public class ProductReportController : Controller
    {
        public ActionResult Index()
        {
            ViewData.Add("Title", "Product Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, object>> modules = this.GenerateProducts();

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            ViewData.Add("Modules", (new SortedByColumnCollection(modules, sortDir, sortKey)).Collection);

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

        public List<Dictionary<String, object>> GenerateProducts()
        {
            List<view_Module> modules = view_Module.getAllModules();
            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            foreach (view_Module module in modules)
            {
                if(module.Expired != null && module.Expired == false && System.Web.HttpContext.Current.GetUser().IfSameArea(module.Area))
                {
                    Dictionary<String, object> dict = new Dictionary<String, object>();

                    dict.Add("article_number", module.Article_number.ToString());
                    dict.Add("name", module.Module);
                    dict.Add("system", module.System);
                    dict.Add("classification", module.Classification);
                    dict.Add("description", module.Description);

                    rows.Add(dict);
                }
            }
            return rows;
        }
    }
}