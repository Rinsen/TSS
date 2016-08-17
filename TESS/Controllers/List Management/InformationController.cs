
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class InformationController : Controller
    {
        // GET: Information
        public ActionResult Index()
        {
            ViewData.Add("SkipProp", new List<String>
            {
                "_ID",
            });
            ViewData.Add("SkipPropEdit", new List<String>
            {
                "_ID",
                "Created",
                "Updated"
            });
            ViewData.Add("Title", "Information Messages");
            ViewData.Add("Properties", typeof(view_Information).GetProperties());

            ViewData.Add("Representatives", view_User.getAllUsers());

            return View();
        }

        public String InformationJsonData()
        {
            this.Response.ContentType = "text/plain";
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Information.getAllInformation()) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0, 0); // not sure how this works with summer time and winter time
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd HH:mm");
            });
        }

        public string GetAllRepresentatives()
        {
            List<String> repNames = new List<string>();
            foreach (view_User user in view_User.getAllUsers())
            {
                repNames.Add(user.Sign);
            }

            return (new JavaScriptSerializer()).Serialize(repNames);
        }

        public String SaveInformation()
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

                view_Information info = new view_Information();
                info.Select("ID = " + variables["_ID"]);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "_ID")
                        info.SetValue(variable.Key, variable.Value);
                }


                info.Update("ID = " + variables["_ID"]);

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String InsertInformation()
        {
            try
            {
                String json = Request.Form["json"];
                Dictionary<String, Object> variables = null;
                view_Information a = null;
                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                    a = (view_Information)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Information));
                }
                catch (Exception e)
                {
                    return "0";
                }

                if(variables["Send_mail_to"] != null)
                {
                    List<view_User> users = new List<view_User>();

                    foreach (String sign in (System.Collections.ArrayList)variables["Send_mail_to"])
                    {
                        view_User user = new view_User();
                        user.Select("Sign=" + sign);
                        users.Add(user);
                    }
                    view_User currUser = System.Web.HttpContext.Current.GetUser();

                    EmailSender es = new EmailSender(currUser, users);
                    es.Send(a.Title, a.Message);
                }
                
                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteInformation()
        {
            try
            {
                String ID = Request.Form["_ID"];
                view_Information a = new view_Information();
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