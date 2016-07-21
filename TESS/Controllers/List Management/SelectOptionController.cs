using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class SelectOptionController : Controller
    {
        // GET: SelectOption
        public ActionResult Index()
        {
            ViewData.Add("SkipProp", new List<String>
            {
                "_ID",
                "Model",
                "Property"
            });

            ViewData.Add("Title", "Information Messages");
            ViewData.Add("Properties", typeof(view_SelectOption).GetProperties());
            ViewData.Add("SelectModels", view_SelectOption.getAllSelectOptionsWhere("Model='view_SelectOption' AND Property='Model'"));
            return View();
        }
        public String SelectOptionJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_SelectOption.getAllSelectOptions()) + "}";
        }

        public String GetTableSelectOptions()
        {
            this.Response.ContentType = "text/plain";
            String model = Request.Form["model"];
            String property = Request.Form["property"];
            if(!String.IsNullOrEmpty(model) || !String.IsNullOrEmpty(property))
            {
                String condition = "Model='" + model + "' AND Property='" + property + "'";
                return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_SelectOption.getAllSelectOptionsWhere(condition)) + "}";
            }
            else
            {
                return "{\"data\":[]}";
            }
        }

        public String GetSelectProperties()
        {
            String model = Request.Form["model"];
            List<view_SelectOption> soList = new List<view_SelectOption>();
            String condition = "Model='" + model + "' AND Property='Property'";
            soList = view_SelectOption.getAllSelectOptionsWhere(condition);
            List<SelectOptions<view_SelectOption>.SelectOption> sooList = new List<SelectOptions<view_SelectOption>.SelectOption>();

            foreach(view_SelectOption so in soList)
            {
                SelectOptions<view_SelectOption>.SelectOption sel;
                sel.Value = so.Value;
                sel.Text = so.Text;
                sooList.Add(sel);
            }

            return (new JavaScriptSerializer()).Serialize(sooList);
        }

        public String SaveSelectOption()
        {
            try
            {
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

                view_SelectOption so = new view_SelectOption();
                so.Select("ID = " + variables["_ID"]);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "_ID")
                        so.SetValue(variable.Key, variable.Value);
                }

                so.Update("ID = " + variables["_ID"]);

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String InsertSelectOption()
        {
            try
            {
                String json = Request.Form["json"];
                view_SelectOption a = null;
                try
                {
                    a = (view_SelectOption)(new JavaScriptSerializer()).Deserialize(json, typeof(view_SelectOption));
                }
                catch (Exception e)
                {
                    return "0";
                }

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteSelectOption()
        {
            try
            {
                String ID = Request.Form["_ID"];
                view_SelectOption a = new view_SelectOption();
                a.Delete("ID = " + ID);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}