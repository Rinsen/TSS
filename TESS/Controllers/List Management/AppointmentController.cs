using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            ViewData["Title"] = "Appointment";
            GlobalVariables.checkIfAuthorized("CustomerContract");
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                this.ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            else
                this.ViewData.Add("Customers", view_Customer.getCustomerNames());
            this.ViewData.Add("Properties", typeof(view_Appointment).GetProperties());
            ViewData.Add("Contacts", view_CustomerContact.getAllCustomerContacts().Where(c => ((List<String>)ViewData["Customers"]).Contains(c.Customer)));
            ViewData.Add("EventTypes", (new view_Appointment()).GetSelectOptions("Event_type"));
            return View();
        }

        public static view_Appointment GetLastVisit(String customer)
        {
            List<view_Appointment> appointments = view_Appointment.getAllAppointments(customer).Where(a => System.Web.HttpContext.Current.GetUser().IfSameArea(a.Area)).ToList();
            appointments.Sort((a, b) => DateTime.Compare(a.Date, b.Date));
            view_Appointment lastVisit = null;
            int i;
            for(i = 0; i < appointments.Count; i++)
            {
                view_Appointment app = appointments[i];
                if(app.Date >= DateTime.Now)
                {
                    if (i > 0)
                    {
                        lastVisit = appointments[i - 1];
                        break;
                    }
                    else
                        break;
                }
            }
            if (lastVisit == null && i != 0)
                lastVisit = appointments.Last();

            return lastVisit;
        }

        public String GetContacts()
        {
            String customer = Request.Form["customer"];
            List<view_CustomerContact> l = view_CustomerContact.getAllCustomerContacts(customer);
            foreach (view_CustomerContact contact in l)
            {
                contact.Customer = System.Web.HttpUtility.HtmlEncode(contact.Customer);
                contact.Contact_person = System.Web.HttpUtility.HtmlEncode(contact.Contact_person);
                contact.Email = System.Web.HttpUtility.HtmlEncode(contact.Email);
            }

            return (new JavaScriptSerializer()).Serialize(l);
        }

        public ActionResult GetiCalendar()
        {
            List<String> customerNames;
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                customerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            else
                customerNames = view_Customer.getCustomerNames();

            String ical = view_Appointment.ParseToiCal(view_Appointment.getAllAppointments().Where(
                a => customerNames.Contains(a.Customer) &&
                System.Web.HttpContext.Current.GetUser().IfSameArea(a.Area)).ToList());

            byte[] byteArray = Encoding.UTF8.GetBytes(ical);
            MemoryStream stream = new MemoryStream(byteArray);

            return File(stream, "text/calendar", "my_appointments_TSS.ics");

        }

        public String AppointmentJsonData()
        {
            this.Response.ContentType = "text/plain";
            List<String> customerNames;
            if(System.Web.HttpContext.Current.GetUser().User_level > 1)
                customerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            else
                customerNames = view_Customer.getCustomerNames();

            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Appointment.getAllAppointments().Where(a=> customerNames.Contains(a.Customer) && System.Web.HttpContext.Current.GetUser().IfSameArea(a.Area))) + "}";

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
            appointment.Customer = customer ?? "";
            appointment.Date = DateTime.Parse(map["Date"]);
            appointment.Event_type = map["Event_type"];
            appointment.Text = map["Text"];
            appointment.Contact_person = map["Contact_person"] ?? "";
            appointment.Area = System.Web.HttpContext.Current.GetUser().Area;
            appointment.Title = map["Title"];
            appointment.Insert();

            return "0";
        }

        public String SaveAppointment()
        {
            try
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

                DateTime dt = DateTime.Parse("2016-06-30");
                String a = dt.ToString();
                view_Appointment appointment = new view_Appointment();
                appointment.Customer = map["Customer"] ?? "";
                appointment.Date = DateTime.Parse(map["Date"]);
                String b = appointment.Date.ToString();
                appointment.Event_type = map["Event_type"];
                appointment.Text = map["Text"];
                appointment._ID = Int32.Parse(map["_ID"]);
                appointment.Title = map["Title"];
                appointment.Contact_person = map["Contact_person"] ?? null;
                appointment.Area = map["Area"];
                appointment.Update("ID=" + appointment._ID);

                return "0";
            }
            catch(Exception e)
            {
                return "-2";
            }
            
        }

        public String GetAppointments()
        {
            String customer = Request.Form["customer"];
            List<view_Appointment> vA = view_Appointment.getAllAppointments(customer).Where(a => (a.Date - DateTime.Now).TotalDays <= 30 
                && (a.Date - DateTime.Now).TotalDays >= 0 && System.Web.HttpContext.Current.GetUser().IfSameArea(a.Area)).OrderBy(a => a.Date).ToList();
            if(vA.Count > 0)
            {
                view_Appointment lastVisit = GetLastVisit(customer);
                vA.Add(lastVisit);
            }

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