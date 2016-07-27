using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class CustomerDivisionReportController : Controller
    {
        // GET: CustomerDivisionReport
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Customer Division Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, object>> customers = this.generateCustomers(Request["user"]);
            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            ViewData.Add("Customers", (new SortedByColumnCollection<Dictionary<String, object>>(customers, sortDir, sortKey)).Collection);
            this.ViewData["Title"] = "Customer Division Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["user"] + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Sale Report\"";

            return pdf;


        }

        public String User()
        {
            String user = Request.Form["user"];
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.generateCustomers(user)) + "}";
        }

        public List<Dictionary<String, object>> generateCustomers(String user)
        {
            List<Dictionary<String, object>> customers = new List<Dictionary<String, object>>();
            List<view_Customer> vCustomers = view_Customer.getAllCustomers(user);
            foreach(view_Customer customer in vCustomers)
            {
                Dictionary<String, object> dict = new Dictionary<String, object>();
                dict.Add("customer", customer.Customer);
                dict.Add("short_name", customer.Short_name);
                dict.Add("customer_type", customer.Customer_type);
                dict.Add("address", customer.Address);
                dict.Add("zip_code", customer.Zip_code);
                dict.Add("it_manager", customer.IT_manager);
                dict.Add("inhabitant_level", customer.Inhabitant_level.ToString());

                customers.Add(dict);
            }

            return customers;
        }
    }
}