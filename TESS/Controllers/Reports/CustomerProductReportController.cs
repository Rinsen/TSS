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
using System.Text;
using System.IO;
using System.Web.UI;
using System.ComponentModel;
using TietoCRM.Models.Interfaces;

namespace TietoCRM.Controllers
{
    public class CustomerProductReportController : Controller
    {
        private static readonly List<String> ignoredProperties = new List<String>()
        {
            "SSMA_timestamp",
            "Customer",
            "Sign",
            "SortNo",
            "Discount_type",
            "Status",
            "Alias"
        };

        // GET: CustomerProductReport
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows();

            List<String> OrderedCustomerNames = view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign);
            OrderedCustomerNames.Sort();

            ViewData.Add("IgnoredProperties", ignoredProperties);
            ViewData.Add("CustomerNames", OrderedCustomerNames);
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Properties", typeof(view_CustomerProductRow).GetProperties());

            this.ViewData["Title"] = "Customer Product Report";

            return View();
        }

        public ActionResult Pdf()
        {
            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(Request["customer"], null);

            // Store all unique Customer name in a set
            HashSet<String> SystemNames = new HashSet<String>();
            foreach (view_CustomerProductRow row in ProductReportRows)
            {
                if(row.Status == "Giltigt")
                    SystemNames.Add(row.SortNo + "#" + row.System);
            }
            List<String> OrderedSystemNames = SystemNames.ToList();

            OrderedSystemNames.Sort();

            ProductReportRows.OrderBy(m => m.SortNo).ThenBy(m => m.Classification).ThenBy(m => m.Article_number);

            ViewData.Add("CustomerProductRows", ProductReportRows);
            ViewData.Add("SystemNames", OrderedSystemNames);
            ViewData.Add("Properties", typeof(view_CustomerProductRow).GetProperties());

            this.ViewData["Title"] = "Customer Product Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["customer"] + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Customer Products\"";

            return pdf;
        }

        public void ExportAsCsv()
        {
            String customer = Request["customer"];
            ViewCsvParser<view_CustomerProductRow> vcp = new ViewCsvParser<view_CustomerProductRow>("CustomerProducts");
            vcp.WriteExcelWithNPOI(view_CustomerProductRow.getAllCustomerProductRows(customer, null));
        }

        public String CustomerData()
        {
            this.Response.ContentType = "text/plain";
            List<view_CustomerProductRow> l = view_CustomerProductRow.getAllCustomerProductRows();
            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = 209715200;
            return "{\"data\":" + js.Serialize(l) + "}";
        }

        public String ContractId()
        {
            String customer = Request.Form["key"];

            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(customer, null);
            HashSet<String> uniqueCPR = new HashSet<String>();

            foreach(view_CustomerProductRow cpr in ProductReportRows)
            {
                if (!String.IsNullOrEmpty(cpr.Alias))
                    cpr.Module = cpr.Alias;

                uniqueCPR.Add(cpr.Contract_id);
            }

            return (new JavaScriptSerializer()).Serialize(uniqueCPR);
        }

        public String CustomerNames()
        {
            String sign = Request.Form["key"];

            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(sign);
            HashSet<String> customerNames = new HashSet<String>();

            foreach (view_CustomerProductRow cpr in ProductReportRows)
            {
                customerNames.Add(cpr.Customer);
            }

            List<String> sortedNames = customerNames.ToList();
            sortedNames.Sort();

            return (new JavaScriptSerializer()).Serialize(sortedNames);
        }

        public String Customer()
        {
            String customer = Request.Form["customer"];

            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(customer, null);

            ProductReportRows.OrderBy(m => m.System);

            
            List<Dictionary<String,String>> rows = new List<Dictionary<String,String>>();
            foreach(view_CustomerProductRow cpr in ProductReportRows)
            {
                if (cpr.Status == "Giltigt")
                {
                    Dictionary<String, String> dic = new Dictionary<String, String>();
                    foreach (System.Reflection.PropertyInfo pi in cpr.GetType().GetProperties())
                    {
                        if (!ignoredProperties.Contains(pi.Name))
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
                                if(pi.GetValue(cpr) != null)
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
    }
}