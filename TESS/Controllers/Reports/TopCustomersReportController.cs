using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class TopCustomersReportController : Controller
    {
        // GET: TopCustomers
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Top Customers Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, Object>> contracts = this.GenerateTopCustomers();
            ViewData.Add("Contracts", contracts);

            this.ViewData["Title"] = "Top Customers Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Top Customers Report\"";

            return pdf;
        }

        public String TopCustomers()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateTopCustomers()) + "}";
        }

        public List<Dictionary<String, Object>> GenerateTopCustomers()
        {
            List<view_Customer> customers = view_Customer.getAllCustomers();
            List<Dictionary<String, Object>> rows = new List<Dictionary<String, Object>>();
            foreach (view_Customer customer in customers)
            {

                Dictionary<String, Object> dict = new Dictionary<string, object>();
                CustomerStatistics stats = new CustomerStatistics(customer, true);


                dict.Add("customer", customer.Customer);
                dict.Add("customer_type", customer.Customer_type);
                dict.Add("amount", Convert.ToInt32(stats.GetTotalSpent(DateTime.Now.Year)));
                dict.Add("county", customer.County.ToString());
                dict.Add("representative", customer.GetReprensentativesAsString());

                rows.Add(dict);
            }
            return rows.OrderByDescending(d => d["amount"]).ToList().GetRange(0,10);
        }
    }
}