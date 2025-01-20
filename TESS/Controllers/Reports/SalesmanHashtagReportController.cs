using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TietoCRM.Models;
using Rotativa.MVC;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using System.IO;
using NPOI.SS.Formula.Functions;

namespace TietoCRM.Controllers
{
    public class SalesmanHashtagReportController : Controller
    {
        private static readonly List<String> ignoredProperties = new List<String>()
        {
            "SSMA_timestamp",
            //"Customer",
            //"Sign",
            //"SortNo",
            //"Discount_type",
            //"Status",
            //"Alias",
            //"Expired",
            //"Read_name_from_module"
        };

        // GET: SalesmanHashtagReport
        public ActionResult Index()
        {
            TietoCRM.Models.GlobalVariables.Initializer();

            ViewData.Add("IgnoredProperties", ignoredProperties);
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Properties", typeof(view_HashtagReport).GetProperties());

            this.ViewData["Title"] = "Salesman Hashtag Report";

            return View();
        }

        public ActionResult Pdf()
        {
            var user = Request["user"];
            var isContract = Request["isContract"];
            var isOffer = Request["isOffer"];

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            var area = System.Web.HttpContext.Current.GetUser().Area;

            var usersDic = (List<string>)new JavaScriptSerializer().Deserialize(user, typeof(List<string>));

            var hashtags = view_HashtagReport.getHashTagReportRows(usersDic, bool.Parse(isContract), bool.Parse(isOffer));

            List<Dictionary<String, object>> offerRows = new List<Dictionary<String, object>>();
            foreach (var hashtag in hashtags.Where(w => w.Offer_number > 0))
            {
                Dictionary<String, object> dict = new Dictionary<String, object>();

                dict.Add("Offer_number", hashtag.Offer_number.ToString());
                dict.Add("Offer_customer", hashtag.Offer_customer);
                dict.Add("Offer_title", hashtag.Offer_title);
                dict.Add("Offer_created", hashtag.Offer_created.ToShortDateString());
                dict.Add("Offer_valid", hashtag.Offer_valid.ToShortDateString());
                dict.Add("Offer_sign", hashtag.Offer_sign);
                dict.Add("Hashtag", hashtag.Hashtag);

                offerRows.Add(dict);
            }

            List<Dictionary<String, object>> contractRows = new List<Dictionary<String, object>>();
            foreach (var hashtag in hashtags.Where(w => w.Offer_number == 0))
            {
                Dictionary<String, object> dict = new Dictionary<String, object>();

                dict.Add("Contract_id", hashtag.Contract_id.ToString());
                dict.Add("Contract_customer", hashtag.Contract_customer);
                dict.Add("Contract_title", hashtag.Contract_title);
                dict.Add("Contract_sign", hashtag.Contract_sign);
                dict.Add("Hashtag", hashtag.Hashtag);

                contractRows.Add(dict);
            }

            if(sortKey == "Hashtag")
            {
                if(offerRows.Count > 0)
                    ViewData.Add("OfferHashtagRows", (new SortedByColumnCollection(offerRows, sortDir, sortKey)).Collection);
                else
                    ViewData.Add("OfferHashtagRows", offerRows);
                if (contractRows.Count > 0)
                    ViewData.Add("ContractHashtagRows", (new SortedByColumnCollection(contractRows, sortDir, sortKey)).Collection);
                else
                    ViewData.Add("ContractHashtagRows", contractRows);
            }
            else if(sortKey.Contains("Contract"))
            {
                if (contractRows.Count > 0)
                    ViewData.Add("ContractHashtagRows", (new SortedByColumnCollection(contractRows, sortDir, sortKey)).Collection);
                else
                    ViewData.Add("ContractHashtagRows", contractRows);
                ViewData.Add("OfferHashtagRows", offerRows);
            }
            else if(sortKey.Contains("Offer"))
            {
                if (offerRows.Count > 0)
                    ViewData.Add("OfferHashtagRows", (new SortedByColumnCollection(offerRows, sortDir, sortKey)).Collection);
                else
                    ViewData.Add("OfferHashtagRows", offerRows);
                ViewData.Add("ContractHashtagRows", contractRows);
            }
            else
            {
                ViewData.Add("OfferHashtagRows", offerRows);
                ViewData.Add("ContractHashtagRows", contractRows);
            }

            ViewData.Add("Properties", typeof(view_HashtagReport).GetProperties());
            ViewData.Add("IgnoredPropertiesExtended", ignoredProperties.ToList());
            ViewData["Title"] = "Salesman Hashtag Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.PageMargins = new Rotativa.Core.Options.Margins(5, 10, 10, 10);

            String headerPath = Server.MapPath("~/Views/CustomerOffer/Header_" + System.Web.HttpContext.Current.GetUser().Sign + ".html").Replace("\\", "/");
            String headerFilePath = "file:///" + headerPath;

            string customSwitches = string.Format("--print-media-type --margin-top 18 --margin-bottom 20 --header-spacing 3 --header-html \"{0}\"", headerFilePath);
            pdf.RotativaOptions.CustomSwitches = customSwitches;
            //pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["customer"] + "\"";
            //pdf.RotativaOptions.CustomSwitches += " --header-center \"Kundens produkter\"";

            var currentUser = System.Web.HttpContext.Current.GetUser();
            FileStream hfs = updateReportHeader(headerPath, currentUser);
            hfs.Close();

            return pdf;
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

        public String GetUserHashtags()
        {
            var user = Request.Form["user"];
            var isContract = Request.Form["contract"];
            var isOffer = Request.Form["offer"];
            var area = System.Web.HttpContext.Current.GetUser().Area;

            var usersDic = (List<string>)new JavaScriptSerializer().Deserialize(user, typeof(List<string>));

            var hashtags = view_HashtagReport.getHashTagReportRows(usersDic, bool.Parse(isContract), bool.Parse(isOffer));
            
            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_HashtagReport cpr in hashtags)
            {
                Dictionary<String, String> dic = new Dictionary<String, String>();
                foreach (System.Reflection.PropertyInfo pi in cpr.GetType().GetProperties())
                {
                    if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
                    {
                        if (pi.GetValue(cpr) != null && ((DateTime)pi.GetValue(cpr)).Year > 1050)
                            dic.Add(pi.Name, ((DateTime)pi.GetValue(cpr)).ToString("yyyy-MM-dd"));
                        else
                            dic.Add(pi.Name, null);
                    }
                    else if(pi.Name == "Offer_number")
                    {
                        if(pi.GetValue(cpr) != null && (int)pi.GetValue(cpr) == 0)
                        {
                            dic.Add(pi.Name, null);
                        }
                        else
                            dic.Add(pi.Name, pi.GetValue(cpr).ToString());
                    }
                    else
                    {
                        if (pi.GetValue(cpr) != null)
                            dic.Add(pi.Name, pi.GetValue(cpr).ToString());
                        else
                            dic.Add(pi.Name, null);
                    }
                }

                rows.Add(dic);
            }

            return "{\"data\":" + (new JavaScriptSerializer()   ).Serialize(rows) + "}";
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