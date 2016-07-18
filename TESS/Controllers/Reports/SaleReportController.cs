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
            List<Dictionary<String, String>> offers = this.generateSaleReport(Request["user"]);
            ViewData.Add("Offers", offers);

            decimal? totalM = 0;
            decimal? totalL = 0;

            foreach(Dictionary<String,String> offer in offers)
            {
                totalM += decimal.Parse(offer["maintenance"].Replace(" ","").Replace("kr",""));
                totalL += decimal.Parse(offer["license"].Replace(" ", "").Replace("kr", ""));
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

        public List<Dictionary<String,String>> generateSaleReport(String user)
        {
            CultureInfo se = CultureInfo.CreateSpecificCulture("sv-SE");
            List<view_Customer> customers = view_Customer.getAllCustomers(user);
            view_User vUser = new view_User();
            vUser.Select("Sign=" + user);
            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_Customer customer in customers)
            {
                Dictionary<String, String> dict = new Dictionary<String, String>();
                decimal? totalMaintenance = 0;
                decimal? totalLicense = 0;
                dict.Add("customer", customer.Customer);
 
                foreach (view_CustomerOffer offer in view_CustomerOffer.getAllCustomerOffers(customer.Customer))
                {
                    if (offer.Offer_status == "Öppen" && vUser.IfSameArea(offer.Area))
                    {
                        foreach (view_OfferRow row in offer._OfferRows)
                        {
                            totalMaintenance += row.Maintenance;
                            totalLicense += row.License;
                        }
                        if(offer.Offer_valid.HasValue && !dict.Keys.Contains("valid_through") || DateTime.Parse(dict["valid_through"]) > offer.Offer_valid.Value)
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
                    }
                }
                dict.Add("maintenance", String.Format(se, "{0:C2}", totalMaintenance).Replace(".", " "));
                dict.Add("license", String.Format(se, "{0:C2}", totalLicense).Replace(".", " "));
                dict.Add("customer_type", customer.Customer_type);
                if (totalMaintenance != 0 || totalLicense != 0)
                    rows.Add(dict);
            }
            return rows;
        }
    }
}