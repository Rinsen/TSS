using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class ServiceController : Controller
    {
        // GET: Service
        public ActionResult Index()
        {

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Service).GetProperties());
            this.ViewData["Title"] = "Services";
            return View();
        }

        public String serviceJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Service.getAllServices()) + "}";
        }

        public String SaveService()
        {
            try
            {
                String code = Request.Form["code"];
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

                view_Service service = new view_Service();
                service.Select("Code = " + code);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    service.SetValue(variable.Key, variable.Value);
                }

                service.Update("Code = " + code);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertService()
        {
            try
            {
                String json = Request.Form["json"];
                view_Service a = null;
                try
                {
                    a = (view_Service)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Service));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Service> services = view_Service.getAllServices();

                a.Code = services[services.Count - 1].Code + 1;

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }
        public String DeleteService()
        {
            try
            {
                string code = Request.Form["code"];
                view_Service a = new view_Service();
                //a.Select("Article_number = " + value);
                a.Delete("Code = " + code);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}