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
    public class CancelledContractsReportController : Controller
    {
        // GET: ClosedContractsReport
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Cancelled Contract Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, object>> contracts = this.GenerateCancelledContracts();

            String sortDir = Request["sort"];
            String sortKey = Request["prop"];

            ViewData.Add("Contracts", (new SortedByColumnCollection(contracts, sortDir, sortKey).Collection));

            this.ViewData["Title"] = "Cancelled Contracts Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Cancelled Contracts Report\"";

            return pdf;


        }

        public String CancelledContracts()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateCancelledContracts()) + "}";
        }

        public List<Dictionary<String, object>> GenerateCancelledContracts()
        {

            List<view_Customer> customers = view_Customer.getAllCustomers();
            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            foreach (view_Customer customer in customers)
            {

                Dictionary<String, object> dict = new Dictionary<String, object>();
                int amountValidContracts = 0;
                List<view_Contract> contracts = view_Contract.GetContracts(customer.Customer);
                foreach (view_Contract contract in contracts)
                {
                    if (contract.Status == "Uppsagt")
                    {
                        amountValidContracts++;
                    }
                }
                dict.Add("customer", customer.Customer);
                dict.Add("customer_type", customer.Customer_type);
                dict.Add("representative", customer.GetReprensentativesAsString());

                List<view_Contract> mainContracts = contracts.Where(c => c.Contract_type == "Huvudavtal" && c.Status == "Uppsagt" && System.Web.HttpContext.Current.GetUser().IfSameArea(c.Area)).ToList();
                if (mainContracts.Count > 0)
                {
                    dict.Add("contact_person", mainContracts[0].Contact_person);
                    dict.Add("main_contract_id", mainContracts[0].Contract_id);
                    dict.Add("amount", amountValidContracts.ToString());
                    rows.Add(dict);
                }
            }
            return rows;
        }
    }
}