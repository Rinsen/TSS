using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class DependencyController : Controller
    {
        // GET: Dependency
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_Dependency).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Dependency).GetProperties());
            this.ViewData["title"] = "Article/Service Dependencies";

            return View();
        }

        public String DependencyJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Dependency.getAllDependencies()) + "}";
        }

        public String SaveDependency()
        {
            try
            {
                var article_number_pk = Request.Form["article_number_pk"];
                var service_number_pk = Request.Form["service_number_pk"];
                var json = Request.Form["json"];

                Dictionary<string, object> variables = null;

                try
                {
                    variables = (Dictionary<string, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<string, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_Dependency dependency = new view_Dependency();
                dependency.Select("Article_number = " + article_number_pk + " And Service_number = " + service_number_pk);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "id_pk")
                        dependency.SetValue(variable.Key, variable.Value);
                }

                dependency.Update("Article_number = " + article_number_pk + " And Service_number = " + service_number_pk);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertDependency()
        {
            try
            {
                String json = Request.Form["json"];
                view_Dependency a = null;
                try
                {
                    a = (view_Dependency)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Dependency));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Dependency> services = view_Dependency.getAllDependencies();

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteDependency()
        {
            try
            {
                var article_number_pk = Request.Form["article_number_pk"];
                var service_number_pk = Request.Form["service_number_pk"];
                view_Dependency a = new view_Dependency();
                //a.Select("Article_number = " + value);
                a.Delete("Article_number = " + article_number_pk + " And Service_number = " + service_number_pk);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}