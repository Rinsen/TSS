using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Extensions;
using System.Data;

namespace TietoCRM.Controllers.List_Management
{
    public class ReminderController : Controller
    {
        // GET: Reminder
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_Reminder).GetProperties();
            this.ViewData["title"] = "Reminders";

            this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            this.ViewData.Add("ControllerName", "CustomerContract");

            this.ViewData.Add("Representatives", view_User.getAllUsers());
            this.ViewData.Add("DefaultSystem", System.Web.HttpContext.Current.GetUser().Area);

            return View();
        }

        public String ReminderJsonData()
        {
            String default_system = Request.Form["default_system"];
            this.Response.ContentType = "text/plain";
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Reminder.getAllReminders(default_system)) + "}";

            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }

        public String SaveReminder()
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

                view_Reminder Reminder = new view_Reminder();
                Reminder.Select("ID_PK = " + id_pk);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "id_pk")
                        Reminder.SetValue(variable.Key, variable.Value);
                }

                Reminder.Update("ID_PK = " + id_pk);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertReminder()
        {
            String default_system = Request.Form["default_system"];
            try
            {
                String json = Request.Form["json"];
                view_Reminder a = null;
                try
                {
                    a = (view_Reminder)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Reminder));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Reminder> services = view_Reminder.getAllReminders(default_system);

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteReminder()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
                view_Reminder a = new view_Reminder();
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