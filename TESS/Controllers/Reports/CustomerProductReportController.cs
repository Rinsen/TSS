﻿using System;
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
            "Alias",
            "Expired",
            "Read_name_from_module"
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
            if (Request["customer"] != "null")
            {
                String area = System.Web.HttpContext.Current.GetUser().Area;
                bool withExpired = Request["expired"] == "Ja" ? true : false;

                List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(Request["customer"], null, area, withExpired, true);

                // Store all unique Customer name in a set
                var SystemNames = new HashSet<string>();
                var MainContracts = new HashSet<string>();
                string oldSystem = "";
                string oldMainContractId = "";

                List<view_Contract> mainContracts = new List<view_Contract>();

                foreach (var row in ProductReportRows)
                {
                    if (row.Main_contract_id != oldMainContractId)
                    {
                        var mainContract = new view_Contract();
                        mainContract.Customer = row.Customer;
                        mainContract.Main_contract_id = row.Main_contract_id;
                        mainContract.Valid_from = row.MainContract_ValidFrom;
                        mainContract.Valid_through = row.MainContract_ValidThrough;
                        mainContract._CustomerProductRows = ProductReportRows.Where(s => s.Status == "Giltigt" && s.Main_contract_id == row.Main_contract_id).ToList();
                        mainContract._MatchedModules = new List<view_Module>();
                        mainContract._OrderedSystemNames = new List<string>();

                        mainContracts.Add(mainContract);

                        oldMainContractId = row.Main_contract_id;
                    }
                }

                List<view_Module> modules = view_Module.getAllModules();

                foreach (var mainContract in mainContracts)
                {
                    var customerProductRows = mainContract._CustomerProductRows.OrderBy(s => s.SortNo);
                    foreach (view_CustomerProductRow row in customerProductRows)
                    {
                        if (row.Status == "Giltigt" && row.System != oldSystem)
                        {
                            SystemNames.Add(row.SortNo + "#" + row.System);
                            oldSystem = row.System;
                        }
                        mainContract._MatchedModules.Add(modules.Where(m => m.Article_number == row.Article_number).First());
                        mainContract._MatchedModules = mainContract._MatchedModules.DistinctBy(m => m.Article_number).ToList();
                    }

                    oldSystem = "";
                    mainContract._OrderedSystemNames = SystemNames.ToList();
                    SystemNames.Clear();

                    string sortDir = Request["sort"];
                    string sortKey = Request["prop"];
                    if(mainContract._CustomerProductRows.Count > 0)
                    {
                        if(string.IsNullOrEmpty(sortDir) && string.IsNullOrEmpty(sortKey)) //No sort
                        {
                            mainContract._CustomerProductRowsSorted = new SortedByColumnCollection(mainContract._CustomerProductRows.ToList<SQLBaseClass>(), "asc", "SortNo").Collection;
                        }
                        else
                        {
                            mainContract._CustomerProductRowsSorted = new SortedByColumnCollection(mainContract._CustomerProductRows.ToList<SQLBaseClass>(), sortDir, sortKey).Collection;
                        }
                    }
                }


                ViewData.Add("MainContracts", mainContracts);
                ViewData.Add("Properties", typeof(view_CustomerProductRow).GetProperties());
                List<String> ignoredPropertiesExtended = ignoredProperties.ToList();
                ignoredPropertiesExtended.Add("System");
                ViewData.Add("IgnoredPropertiesExtended", ignoredPropertiesExtended);
                this.ViewData["Title"] = "Customer Product Report";
                ViewData.Add("ShowClassificationColumn", Request["classificationCol"] != null && Request["classificationCol"] == "true" ? true : false);
                ViewData.Add("ShowContractIdColumn", Request["contractIdCol"] != null && Request["contractIdCol"] == "true" ? true : false);

                ViewAsPdf pdf = new ViewAsPdf("Pdf");
                pdf.RotativaOptions.PageMargins = new Rotativa.Core.Options.Margins(5, 10, 10, 10);

                String headerPath = Server.MapPath("~/Views/CustomerOffer/Header_" + System.Web.HttpContext.Current.GetUser().Sign + ".html").Replace("\\", "/");
                String headerFilePath = "file:///" + headerPath;

                string customSwitches = string.Format("--print-media-type --margin-top 18 --margin-bottom 20 --header-spacing 3 --header-html \"{0}\"", headerFilePath);
                pdf.RotativaOptions.CustomSwitches = customSwitches;
                //pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["customer"] + "\"";
                //pdf.RotativaOptions.CustomSwitches += " --header-center \"Kundens produkter\"";
                
                var user = System.Web.HttpContext.Current.GetUser();
                FileStream hfs = updateReportHeader(headerPath, user);
                hfs.Close();

                return pdf;
            }
            else
            {
                return Content("No customer was chosen");
            }
        }

        public FileStream updateReportHeader(String headerPath, view_User user)
        {
            String headerTxtPath = Server.MapPath("~/Views/CustomerOffer/Header.txt").Replace("\\", "/");
            String content = System.IO.File.ReadAllText(headerTxtPath);
            FileStream fs = new FileStream(headerPath, FileMode.Create, FileAccess.Write);
            content += @"<div class='header-report'>";
            if (user.Use_logo)
            {
                content += @"<div class='logo-report'>
                            <img src='../../Content/img/TE-Lockup-RGB-BLUE.png' />
                            </div><br> ";
            }
            content += @"</div>
                        </html>
                    ";
            StreamWriter writer = new StreamWriter(fs);
            writer.Write(String.Empty);
            writer.Write(content);

            writer.Close();
            return fs;
        }

        public void ExportAsCsv()
        {
            String customer = Request["customer"];
            ViewCsvParser<view_CustomerProductRow> vcp = new ViewCsvParser<view_CustomerProductRow>("CustomerProducts");
            if(System.Web.HttpContext.Current.GetUser().Area == "EDU") //EDU vill ha ALLA status i rapporten, till skillnad från EC och FC som vill att den ska spegla sökresultatet i vyn.
                vcp.WriteExcelWithNPOI(view_CustomerProductRow.getAllCustomerProductRows(customer, null, null, true, true, true));
            else
                vcp.WriteExcelWithNPOI(view_CustomerProductRow.getAllCustomerProductRows(customer, null, null, true, true));
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
                if(!cpr.Read_name_from_module)
                {
                    if (!String.IsNullOrEmpty(cpr.Alias))
                        cpr.Module = cpr.Alias;
                }

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
            String withoutExpired = Request.Form["withoutExpired"];
            String area = System.Web.HttpContext.Current.GetUser().Area;

            List<view_CustomerProductRow> ProductReportRows = view_CustomerProductRow.getAllCustomerProductRows(customer, null, area);

            ProductReportRows.OrderBy(m => m.SortNo);

            
            List<Dictionary<String,String>> rows = new List<Dictionary<String,String>>();
            foreach(view_CustomerProductRow cpr in ProductReportRows)
            {
                if ((cpr.Status == "Giltigt") && (withoutExpired == "false" || (withoutExpired == "true" && !cpr.Expired)))
                {
                    if(!string.IsNullOrEmpty(cpr.Alias) && !cpr.Read_name_from_module)
                    {
                        cpr.Module = cpr.Alias;
                    }

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
        public string ExportExcel()
        {
            bool withExpired = Request["expired"] == "Ja" ? true : false;
            System.Data.DataTable dt = view_CustomerProductRow.ExportCustomerProductsToExcel(Request["customer"], null, null, withExpired);
            TietoCRM.ExportExcel ex = new TietoCRM.ExportExcel();
            return ex.Export(dt, "CustomerProducts.xlsx");

        }
    }
}