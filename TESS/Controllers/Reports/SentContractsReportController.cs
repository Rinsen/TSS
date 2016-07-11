using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Extensions;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Reports
{
    public class SentContractsReportController : Controller
    {
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Sent Contract Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, String>> contracts = this.GenerateSentContracts();
            ViewData.Add("Contracts", contracts);

            this.ViewData["Title"] = "Sent Contracts Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Sent Contracts Report\"";

            return pdf;


        }

        public String SentContacts()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateSentContracts()) + "}";
        }

        public List<Dictionary<String, String>> GenerateSentContracts()
        {
            List<view_Customer> customers;
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                customers = view_Customer.getAllCustomers(System.Web.HttpContext.Current.GetUser().Sign);
            else
                customers = view_Customer.getAllCustomers();

            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_Customer customer in customers)
            {
                int amountValidContracts = 0;
                List<view_Contract> contracts = view_Contract.GetContracts(customer.Customer);
                foreach (view_Contract contract in contracts)
                {
                    if (contract.Status == "Sänt" && System.Web.HttpContext.Current.GetUser().IfSameArea(contract.Area))
                    {
                        Dictionary<String, String> dict = new Dictionary<String, String>();
                        dict.Add("customer", customer.Customer);
                        dict.Add("customer_type", customer.Customer_type);
                        dict.Add("representative", customer.GetReprensentativesAsString());
                        dict.Add("contact_person", contract.Contact_person);
                        dict.Add("contract_id", contract.Contract_id);
                        dict.Add("title", contract.Title);
                        rows.Add(dict);
                    }
                }

            }
            return rows;
        }
    }
}