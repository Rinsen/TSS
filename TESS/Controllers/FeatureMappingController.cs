using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers
{
    public class FeatureMappingController : Controller
    {
        // GET: FeatureMapping
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && m.Expired == false &&
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Feature and Article Mapping";
            return View();
        }

        public String MappingData()
        {
            String articleNumber = Request.Form["module"];
            var test = new Dictionary<String, String>(){
                {"Feature_id", "5" },
                {"Feature", "test" },
            };
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(new List<Dictionary<String, String>>(){ test }) + "}";
        }
    }
}