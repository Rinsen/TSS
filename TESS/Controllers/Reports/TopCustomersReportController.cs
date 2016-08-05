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
            ViewData.Add("Areas", view_Sector.getAllAreas().Where(a => System.Web.HttpContext.Current.GetUser().IfSameArea(a)));
            ViewData.Add("Title", "Top Customers Report");

            return View();
        }

        public String GetUsers()
        {
            String area = Request["area"];

            return (new JavaScriptSerializer()).Serialize(view_User.getAllUsers().Where(u => u.IfSameArea(area)));
        }

        public ActionResult Pdf()
        {
            String user = Request["user"];
            String area = Request["area"];
            String year = Request["year"];
            String sortDirection = Request["sort"];
            String sortKey = Request["prop"];
            List<Dictionary<String, object>> TC = GenerateTopCustomers(user, area, year, 10);

            ViewData.Add("TC", (new SortedByColumnCollection(TC, sortDirection, sortKey)).Collection);

            this.ViewData["Title"] = "Top Customers Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Top Customers Report\"";

            return pdf;
        }

        public String TopCustomers()
        {
            String user = Request["user"];
            String area = Request["area"];
            String year = Request["year"];
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(GenerateTopCustomers(user, area, year, 10)) + "}";
        }

        /// <summary>
        /// Creats a list with customers which has most total spent. 
        /// </summary>
        /// <param name="user">Select customers by this user</param>
        /// <param name="area">Select customers which are in this Area</param>
        /// <param name="year">Which year to select from</param>
        /// <param name="ammount">How many results to return</param>
        /// <returns></returns>
        public static List<Dictionary<String, object>> GenerateTopCustomers(String user, String area, String year, int ammount)
        {
            view_User vUser = new view_User();
            SelectOptions<view_Customer> selectOption = new SelectOptions<view_Customer>();
            List<view_Customer> customers;
            if (area == "*")
                customers = view_Customer.getAllCustomers();
            else if(user == "*")
            {
                customers = new List<view_Customer>();
                List<view_User> users = view_User.getAllUsers().Where(u => u.IfSameArea(area)).ToList();
                foreach(view_User vUser1 in users)
                {
                    customers.AddRange(view_Customer.getAllCustomers(vUser1.Sign));
                }
            }
            else
            {
                vUser.Select("Sign=" + user);
                if (vUser.User_level > 1)
                    customers = view_Customer.getAllCustomers(user);
                else
                    customers = view_Customer.getAllCustomers();
            }
            //SelectOptions<view_Customer> selectOption = new SelectOptions<view_Customer>();
            List<CustomerStatistics> statistics = CustomerStatistics.GetAllCustomerStatstics(customers, int.Parse(year));
            List<Dictionary<String, Object>> rows = new List<Dictionary<string, Object>>();
            /*List<view_User> users = view_User.getAllUsers().Where(u => u.IfSameArea(area)).ToList();
            view_User vUser = new view_User();
            bool a = area == "*";
            bool b = user == "*";
            bool c = false;
            if (!String.IsNullOrEmpty(user) && user != "*")
            {
                vUser.Select("Sign=" + user);
                c = vUser.User_level > 1;
            }*/
                


            foreach (CustomerStatistics statistic in statistics)
            {
                /*if (b && !a)
                {
                    if (!statistic.Customer._Representatives.Any(r => users.Any(u => u.Sign == r)))
                        continue;
                }
                else if (c && !a)
                {
                    if (!statistic.Customer._Representatives.Contains(vUser.Sign))
                        continue;
                }*/
                view_Customer customer = statistic.Customer;

                Dictionary<String, object> dict = new Dictionary<string, object>();

                dict.Add("customer", customer.Customer);
                try
                {
                    dict.Add("amount", Convert.ToInt32(statistic.GetTotalSpent(int.Parse(year), area)));
                }
                catch { }
                dict.Add("representative", customer.GetReprensentativesAsString());
                dict.Add("customer_type", customer.Customer_type);
                dict.Add("county", selectOption.GetValue("County",customer.County.ToString()));

                rows.Add(dict);

            }
            return rows.OrderByDescending(d => d["amount"]).ToList().GetRange(0,Math.Min(ammount,rows.Count));
        }
    }
}