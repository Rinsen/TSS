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

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Module).GetProperties());
            this.ViewData["Title"] = "Services";
            return View();
        }

        public String serviceJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Module.getAllModules(true, 2)) + "}";
        }

        [HttpPost, ValidateInput(false)]
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

                view_Module service = new view_Module();
                service.Select("Article_number = " + code);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    service.SetValue(variable.Key, variable.Value);
                }

                service.Update("Article_number = " + code);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        [HttpPost, ValidateInput(false)]
        public String InsertService()
        {
            try
            {
                String json = Request.Form["json"];
                view_Module a = null;
                try
                {
                    a = (view_Module)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Module));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Module> services = view_Module.getAllModules(false, 2);

                a.Article_number = services[services.Count - 1].Article_number + 1;

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
                view_Module a = new view_Module();
                //a.Select("Article_number = " + value);
                a.Delete("Article_number = " + code);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
        public String GetTinyMCEData()
        {
            string code = Request.Form["artnr"];
            view_Module service = new view_Module();
            service.Select("Article_number = " + code);
            dynamic j = new
            {
                service.Offer_description,
                service.Contract_description
            };
            return (new JavaScriptSerializer()).Serialize(j);
        }
    }
}