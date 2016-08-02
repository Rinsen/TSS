using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class SaleReportController : Controller
    {
        // GET: SaleReport
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Sent Offers Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, object>> offers = this.generateSaleReport(Request["user"]);

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            ViewData.Add("Offers", (new SortedByColumnCollection(offers, sortDir, sortKey)).Collection);

            decimal? totalM = 0;
            decimal? totalL = 0;

            foreach(Dictionary<String, object> offer in offers)
            {
                totalM += (Decimal)offer["maintenance"];
                totalL += (Decimal)offer["license"];
            }
            CultureInfo se = CultureInfo.CreateSpecificCulture("sv-SE");

            ViewData.Add("totalM", String.Format(se, "{0:C2}", totalM).Replace(".", " "));
            ViewData.Add("totalL", String.Format(se, "{0:C2}", totalL).Replace(".", " "));
            this.ViewData["Title"] = "Sent Offers Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + Request["user"] + "\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Sent Offers Report\"";

            return pdf;


        }

        public String User()
        {
            String user = Request.Form["user"];
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.generateSaleReport(user)) + "}";
        }

        public List<Dictionary<String, object>> generateSaleReport(String sign)
        {
            CultureInfo se = CultureInfo.CreateSpecificCulture("sv-SE");
            view_User user = new view_User();
            user.Select("Sign=" + sign);
            List<view_Customer> customers;
            if (user.User_level > 1)
                customers = view_Customer.getAllCustomers(sign);
            else
                customers = view_Customer.getAllCustomers();

            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            foreach (view_Customer customer in customers)
            {
                foreach (view_CustomerOffer offer in view_CustomerOffer.getAllCustomerOffers(customer.Customer))
                {
                    if (offer.Offer_status == "Öppen" && user.IfSameArea(offer.Area))
                    {
                        Dictionary<String, object> dict = new Dictionary<String, object>();
                        decimal? totalMaintenance = 0;
                        decimal? totalLicense = 0;
                        dict.Add("customer", customer.Customer);
                        dict.Add("title", offer.Title);
                        foreach (view_OfferRow row in offer._OfferRows)
                        {
                            totalMaintenance += row.Maintenance;
                            totalLicense += row.License;
                        }
                        if(!dict.Keys.Contains("valid_through") || (String)dict["valid_through"] == "no date found" || DateTime.Parse((String)dict["valid_through"]) > offer.Offer_valid.Value)
                        {
                            if (offer.Offer_created.HasValue)
                                dict["created"] = offer.Offer_created.Value.ToString("yyyy-MM-dd");
                            else
                                dict["created"] = "no date found";

                            if (offer.Offer_valid.HasValue)
                                dict["valid_through"] = offer.Offer_valid.Value.ToString("yyyy-MM-dd");
                            else
                                dict["valid_through"] = "no date found";
                        }
                        dict.Add("maintenance", totalMaintenance);
                        dict.Add("license", totalLicense);
                        dict.Add("customer_type", customer.Customer_type);
                        if(totalMaintenance > 0 || totalLicense > 0)
                            rows.Add(dict);
                    }
                }
            }
            return rows;
        }
    }
}