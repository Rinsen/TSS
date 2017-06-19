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
            ViewData.Add("Systems", GetAllSystemNames(System.Web.HttpContext.Current.GetUser().Area));
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Feature and Article Mapping";
            return View();
        }

        public String MappingData()
        {
            int article_number = -1;
            if(int.TryParse(Request.Form["article_number"], out article_number))
            {
                List<FeatureService.Features> Mapped_Features = view_ModuleFeature.getAllFeatures(article_number);
                List<Dictionary<String, Object>> Return_List = new List<Dictionary<String, Object>>();
                foreach (FeatureService.Features feature in Mapped_Features)
                {
                    Return_List.Add(new Dictionary<String, Object>(){
                        {"Feature_id", feature.Id },
                        {"Feature", feature.Text},
                        {"Warnings", feature.Warnings}, 
                        {"Information", feature.Information}
                    });
                }
                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(Return_List) + "}";
            }
            else
            {
                return "{\"error\", \"Missing article_number parameter or feature_list parameter\"}";
            }

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
                FeatureService.Features[] features = FeatureServiceProxy
                                                    .GetFeaturesClient()
                                                    .GetFeaturesListFlat(productID)
                                                    .Where(f => f.Level != 0)
                                                    .OrderBy(f => f.Id).ToArray();

                foreach (FeatureService.Features feature in features)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach(String option in options)
                    {
                        value.Add(option, feature.GetType().GetProperty(option).GetValue(feature, null));
                    }
                    value.Add("Relation", CreateBreadcrumb(GetRelationByParent(feature)));
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

        public JsonResult GetMappedFeaturesList()
        {
            int article_number = -1;
            if (int.TryParse(Request.Form["article_number"], out article_number))
            {
                List<Dictionary<String, object>> values = new List<Dictionary<string, object>>();
                List<String> options = new List<string>()
                {
                    "Id",
                    "Text",
                    "Information"
                };
                List<FeatureService.Features> mappedFeatures = view_ModuleFeature.getAllFeatures(article_number);

                foreach (FeatureService.Features feature in mappedFeatures)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach (String option in options)
                    {
                        value.Add(option, feature.GetType().GetProperty(option).GetValue(feature, null));
                    }
                    value.Add("Relation", CreateBreadcrumb(GetRelationByParent(feature)));
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
                Response.StatusCode = 400;
                return Json(new Dictionary<String, String>() {
                    {
                        "error", "Missing article_number parameter"
                    }
                });
            }
        }
      
         [HttpPost]
        public JsonResult Map(){

            int article_number = -1;
            if (int.TryParse(Request.Form["article_number"], out article_number) && Request.Form["feature_list"] != null)
            {
                List<int> maplist = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["feature_list"]).ToList();
                view_ModuleFeature moduleFeature = new view_ModuleFeature();
                // First delete all the mappings
                moduleFeature.Delete("Article_number = " + article_number);
                // Insert new mappings   
                foreach (int id in maplist)                         
                {
                    moduleFeature.Feature_Id = id;
                    moduleFeature.Article_number = article_number; 
                    moduleFeature.Insert();
                }

                return Json(new Dictionary<String, String>()
                {
                    {
                        "success", "Features successfully mapped to " + article_number
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

        public JsonResult GetModulesBySystem()
        {
            String system = null;
            
            if (Request.Form["system"] != null)
            {
                system = Request.Form["system"];
                List<view_Module> modules = view_Module.getAllModules()
                                            .Where(m => m.System == system)
                                            .OrderBy(m => m.Article_number)
                                            .ToList();


                return Json(modules);
            }
            else
            {
                return Json(new Dictionary<String, String>()
                {
                    {
                        "error", "No system"
                    }
                });
            }
        }

        public JsonResult GetClassificationSelectOptions()
        {
            String system = null;
            if (Request.Form["system"] != null)
            {
                system = Request.Form["system"];
                String area = System.Web.HttpContext.Current.GetUser().Area;

                return Json(GetAllClassificationNames(area, system));
            }
            else
            {
                return Json(new Dictionary<String, String>()
                {
                    {
                        "error", "No system provided"
                    }
                });
            }
        }

        public JsonResult GetModulesBySystemAndClassification()
        {
            String system = null;
            String classification = null;
            if (Request.Form["system"] != null && Request.Form["classification"] != null)
            {
                system = Request.Form["system"];
                classification = Request.Form["classification"];
                List<view_Module> modules = view_Module.getAllModules()
                                            .Where(m => m.System == system && m.Classification == classification)
                                            .OrderBy(m => m.Article_number)
                                            .ToList();

                return Json(modules);
            }
            else
            {
                return Json(new Dictionary<String, String>()
                {
                    {
                        "error", "No system or classification provided"
                    }
                });
            }
        }

        /// <summary>
        /// Creates a list with the names of all relations, in ascending order, for provided Feature
        /// </summary>
        /// <param name="feature">Feature to look for parent in</param>
        /// <param name="relation">list to build the relations recursively</param>
        /// <returns></returns>
        List<String> GetRelationByParent(FeatureService.Features feature, List<String> relation = null)
        {
            if(relation == null)
            {
                relation = new List<string>();
            }
            if(feature.Parent == null) // base case: continue until Feature has no Parent
            {
                return relation;
            } 
            else // recursive case: add Parent Name to list and continue until base case is fullfilled
            {
                relation.Insert(0, feature.Parent.Text);
                return GetRelationByParent(feature.Parent, relation);
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

        List<SelectListItem> GetAllSystemNames(String area)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area)
                 .DistinctBy(a => a.System)
                 .OrderBy(a => a.SortNo);
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();

            return returnList;
        }
        List<SelectListItem> GetAllClassificationNames(String area, String system)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 .Where(a => a.Area == area && a.System == system)
                 .DistinctBy(a => a.Classification)
                 .OrderBy(a => a.Classification == "-");
                    
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.Classification, Text = a.Classification }).ToList();

            return returnList;
        }
    }
}