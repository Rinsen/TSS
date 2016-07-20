using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TietoCRM.Models;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using System.Dynamic;
using System.Reflection;
using System.Collections.ObjectModel;

namespace TietoCRM.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            view_SelectOption a = new view_SelectOption();
            var test = a.GetSelectOptions("Model");

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
            UserStatistics stats = new UserStatistics(user, true);
            return stats.OpenOffers.ToString();
        }

        public String GetAmountSentContracts()
        {
            view_User user = new view_User();
            user.Select("windows_user='" + User.Identity.Name + "'");
            UserStatistics stats = new UserStatistics(user, true);
            return stats.SentContracts.ToString();
        }

        public String GetAmountExpiringContracts()
        {
            view_User user = new view_User();
            user.Select("windows_user='" + User.Identity.Name + "'");
            TietoCRM.Models.UserStatistics stats = new TietoCRM.Models.UserStatistics(user, true);
            return stats.ExpiringContracts.ToString();

        }

        public string checkReminder()
        {
            view_Reminder vR = new view_Reminder();

            String remindExist = vR.checkIfReminderHighPrio(System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);

            return remindExist;
        }

        public String GetReminders()
        {
            List<view_Reminder> vR = view_Reminder.getRemindersHighPrio(System.Web.HttpContext.Current.GetUser().Area, System.Web.HttpContext.Current.GetUser().Sign);

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

        

        public string GetAllInformation()
        {
            List<view_Information> allInfo = view_Information.getAllValidInformation();
            allInfo.Sort((a,b) => b.Updated.CompareTo(a.Updated));
            List<Dictionary<string, string>> returnList = new List<Dictionary<string, string>>();
            
            foreach (view_Information info in allInfo)
            {
               
                Dictionary<string, string> returnDic = new Dictionary<string, string>();
                
                returnDic.Add("AuthorFullName", info.getAuthorName());
                foreach(PropertyInfo pi in typeof(view_Information).GetProperties())
                {
                    if(pi.Name == "Created" || pi.Name == "Updated")
                        returnDic.Add(pi.Name, ((DateTime)pi.GetValue(info, null)).ToString("yyyy-MM-dd HH:mm"));
                    else
                        returnDic.Add(pi.Name, pi.GetValue(info, null).ToString());
                }    
                returnList.Add(returnDic);
            }

            String jsonData = (new JavaScriptSerializer()).Serialize(returnList);
            return jsonData;
        }

    }
}
