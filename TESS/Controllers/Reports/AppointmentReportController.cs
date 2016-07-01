using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class AppointmentReportController : Controller
    {
        // GET: AppointmentReport
        public ActionResult Index()
        {
            ViewData.Add("Title", "AppointmentReport");
            ViewData.Add("Unprintable", new List<String> {"_ID"});
            ViewData.Add("Properties", typeof(view_Appointment).GetProperties());
            ViewData.Add("Users", view_User.getAllUsers());

            return View();
        }

        public ActionResult Pdf(String start, String stop, String user)
        {
            ViewData.Add("Title", "AppointmentReport");
            ViewData.Add("Unprintable", new List<String> { "_ID" });
            ViewData.Add("Properties", typeof(view_Appointment).GetProperties());

            DateTime Start;
            DateTime Stop;
            ViewAsPdf pdf = new ViewAsPdf("Pdf");

            if (IsValidSqlDateTimeNative(start) && IsValidSqlDateTimeNative(stop))
            {
                ViewData.Add("ValidDate", true);
                Start = Convert.ToDateTime(start);
                Stop = Convert.ToDateTime(stop);

                List<String> customerNames = view_Customer.getCustomerNames(Request["user"]);
                ViewData.Add("Appointments", view_Appointment.getAllAppointments().Where(a => a.Date <= Stop && a.Date >= Start && customerNames.Contains(a.Customer)));

                pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \" \"";
                pdf.RotativaOptions.CustomSwitches += " --header-center \" Appointments between " + Start.ToString("yyyy-MM-dd") + " and " + Stop.ToString("yyyy-MM-dd") + " \"";
            }
            else
            {
                ViewData.Add("ValidDate", false);
            }


            return pdf;
        }

        public bool IsValidSqlDateTimeNative(string someval)
        {
            bool valid = false;
            DateTime testDate = DateTime.MinValue;
            System.Data.SqlTypes.SqlDateTime sdt;
            if (DateTime.TryParse(someval, out testDate))
            {
                try
                {
                    // take advantage of the native conversion
                    sdt = new System.Data.SqlTypes.SqlDateTime(testDate);
                    valid = true;
                }
                catch (System.Data.SqlTypes.SqlTypeException ex)
                {

                    // no need to do anything, this is the expected out of range error
                }
            }

            return valid;
        }

        public String GetAppointments()
        {
            this.Response.ContentType = "text/plain";
            List<String> customerNames = view_Customer.getCustomerNames(Request["user"]);
            DateTime start = DateTime.Parse(Request["start"]);
            DateTime stop = DateTime.Parse(Request["stop"]);
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Appointment.getAllAppointments().Where(a => a.Date <= stop && a.Date >= start && customerNames.Contains(a.Customer))) + "}";

            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0, 0); // not sure how this works with summer time and winter time
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd HH:mm");
            });
        }
    }
}