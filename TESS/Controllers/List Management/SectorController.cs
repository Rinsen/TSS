using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class SectorController : Controller
    {
        // GET: Sector
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_Sector).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Sector).GetProperties());
            this.ViewData["title"] = "Sectors";

            return View();
        }

        public String SectorJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Sector.getAllSectors()) + "}";
        }

        public String SaveSector()
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

                view_Sector Sector = new view_Sector();
                Sector.Select("ID_PK = " + id_pk);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "id_pk")
                        Sector.SetValue(variable.Key, variable.Value);
                }

                Sector.Update("ID_PK = " + id_pk);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertSector()
        {
            try
            {
                String json = Request.Form["json"];
                view_Sector a = null;
                try
                {
                    a = (view_Sector)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Sector));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Sector> services = view_Sector.getAllSectors();

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteSector()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
                view_Sector a = new view_Sector();
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