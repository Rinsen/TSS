using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_User).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_User).GetProperties());
            this.ViewData["title"] = "User";

            return View();
        }

        public String UserJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_User.getAllUsers()) + "}";
        }

        public String SaveUser()
        {
            try
            {
                String sign = Request.Form["sign"];
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

                view_User user = new view_User();
                user.Select("Sign = " + sign);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if(variable.Key != "Sign")
                        user.SetValue(variable.Key, variable.Value);
                }

                user.Update("Sign = " + sign);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertUser()
        {
            try
            {
                String json = Request.Form["json"];
                view_User a = null;
                try
                {
                    a = (view_User)(new JavaScriptSerializer()).Deserialize(json, typeof(view_User));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_User> services = view_User.getAllUsers();

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteUser()
        {
            try
            {
                String sign = Request.Form["sign"];
                view_User a = new view_User();
                //a.Select("Article_number = " + value);
                a.Delete("Sign = " + sign);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}