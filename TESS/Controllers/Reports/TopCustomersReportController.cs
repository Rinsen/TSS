using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            List<Dictionary<String, object>> TC = GenerateTopCustomers(user, area, year, 400);

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
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(GenerateTopCustomers(user, area, year, 400)) + "}";
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
            //view_User vUser = new view_User();
            //SelectOptions<view_Customer> selectOption = new SelectOptions<view_Customer>();
            //List<view_Customer> customers;
            //if (area == "*")
            //    customers = view_Customer.getAllCustomers();
            //else if(user == "*")
            //{
            //    customers = new List<view_Customer>();
            //    List<view_User> users = view_User.getAllUsers().Where(u => u.IfSameArea(area)).ToList();
            //    foreach(view_User vUser1 in users)
            //    {
            //        customers.AddRange(view_Customer.getAllCustomers(vUser1.Sign));
            //    }
            //}
            //else
            //{
            //    vUser.Select("Sign=" + user);
            //    if (vUser.User_level > 1)
            //        customers = view_Customer.getAllCustomers(user);
            //    else
            //        customers = view_Customer.getAllCustomers();
            //}

            //List<CustomerStatistics> statistics = CustomerStatistics.GetAllCustomerStatstics(customers, int.Parse(year));
            //List<Dictionary<String, Object>> rows = new List<Dictionary<string, Object>>();

            //foreach (CustomerStatistics statistic in statistics)
            //{
            //    view_Customer customer = statistic.Customer;

            //    Dictionary<String, object> dict = new Dictionary<string, object>();
            //    try
            //    {
            //        dict.Add("customer", customer.Customer);
            //        dict.Add("amount", Convert.ToInt32(statistic.GetTotalSpent(int.Parse(year), area)));
            //        dict.Add("representative", customer.GetReprensentativesAsString());
            //        dict.Add("customer_type", customer.Customer_type);
            //        dict.Add("county", selectOption.GetValue("County",customer.County.ToString()));

            //        rows.Add(dict);
            //    }
            //    catch { }

            //}
            SelectOptions<view_Customer> selectOption = new SelectOptions<view_Customer>();
            List<Dictionary<String, Object>> rows = new List<Dictionary<string, Object>>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();
                String query = "stp_TopCustomer";

                SqlCommand command = new SqlCommand(query, connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Prepare();
                command.Parameters.AddWithValue("@pYear", int.Parse(year));
                command.Parameters.AddWithValue("@pSign", user);
                command.Parameters.AddWithValue("@pArea", area);
                command.Parameters.AddWithValue("@pAntal", ammount);
                //command.Parameters.AddWithValue("@area", user.Area);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, object> dict = new Dictionary<string, object>();
                            try
                            {
                                dict.Add("customer", reader.GetString(0));
                                dict.Add("amount", Convert.ToInt32(reader.GetValue(5)));
                                dict.Add("customer_type", reader.GetString(1));
                                dict.Add("county", reader.GetString(2));
                                dict.Add("representative", reader.GetString(6));
                                rows.Add(dict);
                            }
                            catch { }
                        }
                    }
                }
            }
            return rows.OrderByDescending(d => d["amount"]).ToList();
        }
    }
}