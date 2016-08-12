using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TietoCRM.Models;
using Rotativa.Core;
using Rotativa.MVC;
using System.Security.Principal;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;

namespace TietoCRM.Controllers
{
    public class CustomerMissingProductReportController : Controller
    {
        // GET: CustomerProductReport
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<String> OrderedCustomerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            OrderedCustomerNames.Sort();

            ViewData.Add("CustomerNames", OrderedCustomerNames);
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Properties", typeof(view_CustomerMissingProductReport).GetProperties());

            this.ViewData["Title"] = "Missing Products Report";

            return View();
        }

        public ActionResult Pdf()
        {
            List<view_CustomerMissingProductReport> CustomerMissingProducts = view_CustomerMissingProductReport.getCustomerMissingProducts(Request["customer"]);

            // Store all unique Customer name in a set
            HashSet<String> SystemNames = new HashSet<String>();
            foreach (view_CustomerMissingProductReport row in CustomerMissingProducts)
            {
                SystemNames.Add(row.Fixed_price + "#" + row.System);
            }
            List<String> OrderedSystemNames = SystemNames.ToList();

            OrderedSystemNames.Sort();

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            SortedByColumnCollection scc = new SortedByColumnCollection(CustomerMissingProducts, sortDir, sortKey);

            
            ViewData.Add("CustomerMissingProducts", scc.Collection);
            ViewData.Add("SystemNames", OrderedSystemNames);
            ViewData.Add("Properties", typeof(view_CustomerMissingProductReport).GetProperties());

            this.ViewData["Title"] = "Missing Products Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["customer"] + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Missing Products\"";

            return pdf;


        }
        public String GetData()
        {
            String sign = Request.Form["user"];
            String customer = Request.Form["customer"];

            view_User user = new view_User();
            user.Select("Sign=" + sign);

            List<view_CustomerMissingProductReport> ProductReportRows = view_CustomerMissingProductReport.getCustomerMissingProducts(customer);

            ProductReportRows.OrderBy(m => m.Classification).ThenBy(m => m.Status).ThenBy(m => m.Article_number);

            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_CustomerMissingProductReport cpr in ProductReportRows)
            {
                if(user.IfSameArea(cpr.Area))
                {
                    Dictionary<String, String> dic = new Dictionary<String, String>();
                    foreach (System.Reflection.PropertyInfo pi in cpr.GetType().GetProperties())
                    {
                        if (pi.Name != "SSMA_timestamp" && pi.Name != "Customer" && pi.Name != "Sign")
                        {
                            if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
                            {
                                if (pi.GetValue(cpr) != null)
                                    dic.Add(pi.Name, ((DateTime)pi.GetValue(cpr)).ToString("yyyy-MM-dd"));
                                else
                                    dic.Add(pi.Name, null);
                            }
                            else
                            {
                                if (pi.GetValue(cpr) != null)
                                    dic.Add(pi.Name, pi.GetValue(cpr).ToString());
                                else
                                    dic.Add(pi.Name, null);
                            }
                        }
                    }
                    rows.Add(dic);
                }
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(rows) + "}";
        }
        public String CustomerNames()
        {
            String sign = Request.Form["key"];

            List<String> customerNames = view_Customer.getCustomerNames(sign);

            List<String> sortedNames = customerNames.ToList();
            sortedNames.Sort();

            return (new JavaScriptSerializer()).Serialize(sortedNames);
        }
    }
}