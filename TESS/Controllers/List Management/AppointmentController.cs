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
    public class AppointmentController : Controller
    {
        // GET: Appointment
        public ActionResult Index()
        {
            GlobalVariables.checkIfAuthorized("CustomerContract");
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            else
                this.ViewData.Add("Customers", view_Customer.getCustomerNames());
            this.ViewData.Add("Properties", typeof(view_Appointment).GetProperties());
            return View();
        }

        public String AppointmentJsonData()
        {
            String customer = Request.Form["customer"];
            this.Response.ContentType = "text/plain";
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Appointment.getAllAppointments(customer)) + "}";

            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0, 0); // not sure how this works with summer time and winter time
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd HH:mm");
            });
        }

        public String InsertAppointment()
        {
            String json = Request["json"];
            String customer = Request["customer"];
            Dictionary<String, dynamic> map = null;
            try
            {
                map = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
            }
            catch (Exception e)
            {
                return "-1";
            }

            view_Appointment appointment = new view_Appointment();
            appointment.Customer = customer;
            appointment.Date = DateTime.Parse(map["Date"]);
            appointment.Event_type = map["Event_type"];
            appointment.Text = map["Text"];
            appointment.Insert();

            return "0";
        }

        public String SaveAppointment()
        {
            String json = Request["json"];
            String customer = Request["customer"];
            Dictionary<String, dynamic> map = null;
            try
            {
                map = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
            }
            catch (Exception e)
            {
                return "-1";
            }

            view_Appointment appointment = new view_Appointment();
            appointment.Customer = map["Customer"];
            appointment.Date = DateTime.Parse(map["Date"]);
            appointment.Event_type = map["Event_type"];
            appointment.Text = map["Text"];
            appointment._ID = Int32.Parse(map["_ID"]);
            appointment.Update("ID=" + appointment._ID);

            return "0";
        }

        public String GetAppointments()
        {
            String customer = Request.Form["customer"];
            List<view_Appointment> vA = view_Appointment.getAllAppointments(customer).Where(a => (a.Date - DateTime.Now).TotalDays <= 30 && (a.Date - DateTime.Now).TotalDays >= 0).OrderBy(a => a.Date).ToList();

            String jsonData = (new JavaScriptSerializer()).Serialize(vA);
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0, 0); // not sure how this works with summer time and winter time
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd HH:mm");
            });
        }

        public String DeleteAppointment()
        {
            try
            {
                String id = Request.Form["ID"];
                view_Appointment a = new view_Appointment();
                a.Delete("ID = " + id);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}