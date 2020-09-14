using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class PopulationController : Controller
    {
        // GET: Population
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_Population).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Population).GetProperties());
            this.ViewData["title"] = "Population Levels";

            return View();
        }

        public String PopulationJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Population.getAllPopulations()) + "}";
        }

        public String SavePopulation()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_Population Population = new view_Population();
                Population.Select("ID_PK = " + id_pk);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "id_pk")
                        Population.SetValue(variable.Key, variable.Value);
                }

                Population.Update("ID_PK = " + id_pk);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertPopulation()
        {
            try
            {
                String json = Request.Form["json"];
                view_Population a = null;
                try
                {
                    a = (view_Population)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Population));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Population> services = view_Population.getAllPopulations();

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeletePopulation()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
                view_Population a = new view_Population();
                //a.Select("Article_number = " + value);
                a.Delete("ID_PK = " + id_pk);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}