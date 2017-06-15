using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            ViewData.Add("Products", new ObservableCollection<FeatureService.Product>(FeatureServiceProxy.GetProductClient().GetProducts()));
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

        public JsonResult GetFeatures()
        {
            int productID = -1;
            if(int.TryParse(Request.Form["productID"], out productID))
            {
                return Json(FeatureServiceProxy.GetFeaturesClient().GetFeatures(productID));
            }
            else
            {
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing productID parameter"
                    }
                }); 
            }
        }
        public JsonResult GetFeaturesList()
        {
            int productID = -1;
            if (int.TryParse(Request.Form["productID"], out productID))
            {

                List<String> options = new List<string>()
                {
                    "Id",
                    "Text",
                    "Information"
                };
                List<Dictionary<String, object>> values = new List<Dictionary<string, object>>();
                FeatureService.Features[] features = FeatureServiceProxy.GetFeaturesClient().GetFeatures(productID);

                List<FeatureService.Features> featuresWithChildren = GetAllFeatureChildren(features.ToList());
                featuresWithChildren = featuresWithChildren.OrderBy(f => f.Id).ToList();
                foreach (FeatureService.Features feature in featuresWithChildren)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach(String option in options)
                    {
                        value.Add(option, feature.GetType().GetProperty(option).GetValue(feature, null));
                    }
                    value.Add("Relation", CreateBreadcrumb(GetRelationByParentId(new List<string>(), feature.ParentId, featuresWithChildren)));
                    values.Add(value);
                }
                return Json(new Dictionary<String, object>() {
                    {
                        "options", options
                    },
                    {
                        "values", values
                    }
                });
            }
            else
            {
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing productID parameter"
                    }
                });
            }
        }
        /* 1. Verifiera innehållet
         * 2. parsa listan till en List<int> med featureids
         * 3. skapa nya view_featuremapping för varje id
         * 4. inserta till databas
         * 5. returnera hur det gick
         */
        public JsonResult Map(){

            int article_number = -1;
            if (int.TryParse(Request.Form["article_number"], out article_number) && Request.Form["feature_list"] != null)
            {
                List<int> maplist = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["article_number"]);
                var map = new view_ModuleFeature();
                map.Delete("article_number = " + article_number);   // first delete all the mappings
                foreach (int id in maplist)                         // insert new mappings
                {
                    map.Feature_Id = id;
                    map.Article_number = article_number; 
                    map.Insert();
                }

                return Json(new Dictionary<String, String>()
                {
                    {
                        "success", "Featues successfully mapped to" + article_number
                    }
                });
            }
            else
            {
                Response.StatusCode = 400;
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing article_number parameter or feature_list parameter"
                    }
                });
            }
        }

        List<FeatureService.Features> GetAllFeatureChildren(List<FeatureService.Features> features)
        {
            List<FeatureService.Features> children = new List<FeatureService.Features>();
            foreach(FeatureService.Features feature in features)
            {
                children.Add(feature);
                if (feature.Children.Length > 0)
                {
                    children = children.Concat(GetAllFeatureChildren(feature.Children.ToList()).ToList()).ToList();
                }
            }
            return children;
        }

        List<String> GetRelationByParentId(List<String> parents, int parentId, List<FeatureService.Features> list)
        {
            List<String> returnList = new List<string>();
            if(parentId == 0)
            {
                return parents;
            } else
            {
                FeatureService.Features parent = list.Where(f => f.Id == parentId).First();
                returnList.Add(parent.Text);
                returnList = returnList.Concat(parents).ToList();
                return GetRelationByParentId(returnList, parent.ParentId, list);
            }
        }

        String CreateBreadcrumb(List<String> items)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<ol class='breadcrumb'>");
            foreach(String item in items)
            {
                sb.Append("<li class='breadcrumb-item'>" + item + "</li>");
            }
            sb.Append("</ol>");
            return sb.ToString();
        }
    }
}