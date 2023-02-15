using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers
{
    public class ModuleMappingController : Controller
    {
        // GET: ModuleMapping
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_Module> modules = view_Module.getAllModules();
            modules = modules.Where(m => m.Discount_type == 0 && m.Expired == false &&
                System.Web.HttpContext.Current.GetUser().IfSameArea(m.Area)).ToList();

            ViewData.Add("Modules", modules);
            ViewData.Add("Systems", GetAllSystemNames(System.Web.HttpContext.Current.GetUser().Area));
            //ViewData.Add("Properties", typeof(view_Module).GetProperties());
            this.ViewData["Title"] = "Article and Service Dependency";
            return View();
        }

        public String MappingData()
        {
            int parent_article_number = -1;
            if (int.TryParse(Request.Form["parent_article_number"], out parent_article_number))
            {
                List<view_Module> mappedArticles = view_ModuleModule.getAllChildModules(parent_article_number);
                List<Dictionary<String, Object>> Return_List = new List<Dictionary<String, Object>>();
                foreach (view_Module article in mappedArticles)
                {
                    Return_List.Add(new Dictionary<String, Object>(){
                        {"Article_number", article.Article_number },
                        {"Module", article.Module},
                        {"System", article.System},
                        {"Classification", article.Classification},
                        {"Module_type", article.Module_type == 1 ? "Article" : "Service"}
                    });
                }
                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(Return_List) + "}";
            }
            else
            {
                return "{\"data\":" + new JavaScriptSerializer().Serialize(new List<Dictionary<string, object>>()) + "}";
            }

        }

        public JsonResult GetModulesList()
        {
            string system = null;
            string mapType = null;

            if (Request.Form["system"] != null)
            {
                system = Request.Form["system"];
                List<String> options = new List<string>()
                {
                    "Article_number",
                    "Module"
                };
                List<Dictionary<String, object>> values = new List<Dictionary<string, object>>();
                List<view_Module> modules = view_Module
                                                    .getAllModules(false,0)
                                                    .Where(m => m.System == system)
                                                    .OrderBy(m => m.Article_number).ToList();

                foreach (view_Module module in modules)
                {
                    Dictionary<String, object> value = new Dictionary<string, object>();
                    foreach (String option in options)
                    {
                        value.Add(option, module.GetType().GetProperty(option).GetValue(module, null));
                    }
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
                return Json(new Dictionary<String, String>()
                {
                    {
                        "error", "No system provided"
                    }
                });
            }
        }

        public JsonResult GetMappedModulesList()
        {
            int parent_article_number;
            if (int.TryParse(Request.Form["parent_article_number"], out parent_article_number))
            {
                List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
                List<string> options = new List<string>()
                    {
                        "Article_number",
                        "Module"
                    };

                List<view_Module> mappedModules = view_ModuleModule.getAllChildModules(parent_article_number);

                foreach (view_Module module in mappedModules)
                {
                    Dictionary<string, object> value = new Dictionary<string, object>();
                    foreach (string option in options)
                    {
                        value.Add(option, module.GetType().GetProperty(option).GetValue(module, null));
                    }
                    values.Add(value);
                }

                return Json(new Dictionary<string, object>() {
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
                return Json(new Dictionary<string, string>()
                {
                    {
                        "error", "Missing parent_article_number parameter"
                    }
                });
            }
        }

        [HttpPost]
        public JsonResult Map()
        {

            int parent_article_number = -1;
            if (int.TryParse(Request.Form["parent_article_number"], out parent_article_number) && Request.Form["mapped_articles"] != null)
            {
                List<int> maplist = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["mapped_articles"]).ToList();
                view_ModuleModule moduleModule = new view_ModuleModule();
                // First delete all the mappings
                moduleModule.Delete("Parent_article_number = " + parent_article_number);
                // Insert new mappings   
                foreach (int article_number in maplist)
                {
                    var module = new view_Module();
                    module.Select("Article_number = " + article_number);

                    moduleModule.Parent_article_number = parent_article_number;
                    moduleModule.Article_number = article_number;
                    moduleModule.Module_type = module.Module_type;
                    moduleModule.Insert();
                }

                return Json(new Dictionary<String, String>()
                {
                    {
                        "success", "Features successfully mapped to " + parent_article_number
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

        /// <summary>
        /// Creates a list with the names of all relations, in ascending order, for provided Feature
        /// </summary>
        /// <param name="feature">Feature to look for parent in</param>
        /// <param name="relation">list to build the relations recursively</param>
        /// <returns></returns>
        List<String> GetRelationByParent(FeatureService.Features feature, List<String> relation = null)
        {
            if (relation == null)
            {
                relation = new List<string>();
            }
            if (feature.Parent == null) // base case: continue until Feature has no Parent
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
            foreach (String item in items)
            {
                sb.Append("<li class='breadcrumb-item'>" + item + "</li>");
            }
            sb.Append("</ol>");
            return sb.ToString();
        }
        List<SelectListItem> GetAllSystemNames(String area)
        {
            IEnumerable<view_Sector> allSectors = view_Sector.getAllSectors()
                 //.Where(a => a.Area == area)
                 .DistinctBy(a => a.System)
                 .OrderBy(a => a.SortNo);
            List<SelectListItem> returnList = allSectors.Select(a => new SelectListItem { Value = a.System, Text = a.System }).ToList();

            return returnList;
        }

    }
}