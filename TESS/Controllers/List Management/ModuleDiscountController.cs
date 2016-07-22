using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class ModuleDiscountController : Controller
    {
        // GET: ModuleDiscount
        public ActionResult Index()
        {
            ViewData.Add("Title", "Module Discount");
            view_User user = System.Web.HttpContext.Current.GetUser();
            ViewData.Add("User", user);

            ViewData.Add("Properties", typeof(view_ModuleDiscount).GetProperties());
            List<view_Module> modules = view_Module.getAllModules().Where(m => m.Expired == false &&
                user.IfSameArea(m.Area)).OrderBy(m => m.Area).ThenBy(m => m.Article_number).ToList();

            ViewData.Add("Modules", modules);
            return View();
        }

        public String ModuleData()
        {
            this.Response.ContentType = "text/plain";
            view_User user = System.Web.HttpContext.Current.GetUser();
            List<view_ModuleDiscount> modules;
            if (user.User_level == 2)
                modules = view_ModuleDiscount.GetAllModuleDiscounts(user.Area);
            else
                modules = view_ModuleDiscount.GetAllModuleDiscounts();
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(modules) + "}";

            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0, 0); // not sure how this works with summer time and winter time
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }

        public String InsertModule()
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

                view_ModuleDiscount module = new view_ModuleDiscount();
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        module.SetValue(variable.Key, variable.Value);
                    }
                    if(System.Web.HttpContext.Current.GetUser().Area != "*")
                        module.Area = System.Web.HttpContext.Current.GetUser().Area;

                    module.Delete("Article_number=" + module.Article_number + " AND Area=" + module.Area);
                    module.Insert();

                    return "1";
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }

        }
        public String SaveModule()
        {
            try
            {
                String json = Request.Form["json"];
                String id = Request.Form["id"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_ModuleDiscount module = new view_ModuleDiscount();
                module.Select("ID = " + id);
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        module.SetValue(variable.Key, variable.Value);
                    }
                    if (System.Web.HttpContext.Current.GetUser().Area != "*")
                        module.Area = System.Web.HttpContext.Current.GetUser().Area;
                    module.Update("ID = " + id);

                    return "1";
                }
                catch (Exception ex)
                {
                    return "0";
                }
            }
            catch
            {
                return "-1";
            }
        }

        public String DeleteModule()
        {
            try
            {
                String id = Request.Form["id"];

                view_ModuleDiscount module = new view_ModuleDiscount();
                module.Delete("ID = " + id);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }
    }
}