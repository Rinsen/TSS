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
    public class ValidContractsReportController : Controller
    {
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Valid Contract Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, object>> contracts = this.GenerateValidContracts();
            String sortDirection = Request["sort"];
            String sortKey = Request["prop"];
            ViewData.Add("Contracts", (new SortedByColumnCollection<Dictionary<String, object>>(contracts, sortDirection, sortKey)).Collection);

            this.ViewData["Title"] = "Valid Contracts Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Valid Contracts Report\"";

            return pdf;


        }

        public String ValidContacts()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateValidContracts()) + "}";
        }

        public List<Dictionary<String, object>> GenerateValidContracts()
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
                    if (contract.Status == "Giltigt")
                    {
                        amountValidContracts++;
                    }
                }
                dict.Add("customer", customer.Customer);
                dict.Add("customer_type", customer.Customer_type);
                dict.Add("representative", customer.Representative);
                dict.Add("it_manager", customer.IT_manager);

                List<view_Contract> mainContracts = contracts.Where(c => c.Contract_type == "Huvudavtal" && c.Status == "Giltigt" &&  System.Web.HttpContext.Current.GetUser().IfSameArea(c.Area)).ToList();
                if(mainContracts.Count > 0)
                {
                    dict.Add("main_contract_id", mainContracts[0].Contract_id);
                    dict.Add("amount", amountValidContracts);
                    rows.Add(dict);
                }
            }
            return rows;
        }
    }
}