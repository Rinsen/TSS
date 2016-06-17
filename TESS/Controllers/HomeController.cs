using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TietoCRM.Models;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;


namespace TietoCRM.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var cName = User.Identity.Name;
            this.ViewData["Title"] = cName;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contract page.";

            return View();
        }

        public String GetAmountOpenOffers()
        {
            view_User user = new view_User();
            user.Select("windows_user='" + User.Identity.Name + "'");
            Statistics stats = new Statistics(user);
            return stats.getAmountOpenOffers().ToString();
        }

        public String GetAmountSentContracts()
        {
            view_User user = new view_User();
            user.Select("windows_user='" + User.Identity.Name + "'");
            Statistics stats = new Statistics(user);
            return stats.getAmounSentContracts().ToString();
        }

        public String GetAmountExpiringContracts()
        {
            view_User user = new view_User();
            user.Select("windows_user='" + User.Identity.Name + "'");
            Statistics stats = new Statistics(user);
            return stats.getAmountExpiringContracts().ToString();

        }

        public string checkReminder()
        {
            view_Reminder vR = new view_Reminder();

            String remindExist = vR.checkIfReminderHighPrio(System.Web.HttpContext.Current.GetUser().Default_system, System.Web.HttpContext.Current.GetUser().Sign);

            return remindExist;
        }

        public String GetReminders()
        {
            List<view_Reminder> vR = view_Reminder.getRemindersHighPrio(System.Web.HttpContext.Current.GetUser().Default_system, System.Web.HttpContext.Current.GetUser().Sign);

            foreach (view_Reminder v in vR)
            {
                v.Customer_name = System.Web.HttpUtility.HtmlEncode(v.Customer_name);
            }

            String jsonData = (new JavaScriptSerializer()).Serialize(vR);
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
            //return (new JavaScriptSerializer()).Serialize(vR);

        }

        public string DeactivateReminder()
        {
            String id = Request.Form["id_pk"];
            view_Reminder a = new view_Reminder();

            a.deactivateReminder(Int32.Parse(id));

            return "1";

        }

    }
}