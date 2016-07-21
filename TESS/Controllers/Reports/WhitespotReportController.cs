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
    public class WhitespotReportController : Controller
    {
        // GET: WhitespotReport
        public ActionResult Index()
        {
            ViewData.Add("Users", view_User.getAllUsers());
            ViewData.Add("Title", "Whitespot Report");

            return View();
        }


        public ActionResult Pdf()
        {
            List<Dictionary<String, String>> contracts = this.GenerateValidContracts();
            ViewData.Add("Contracts", contracts);

            this.ViewData["Title"] = "Whitespot Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Valid Contracts Report\"";

            return pdf;
        }

        public String Contracts()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateValidContracts()) + "}";
        }

        public List<Dictionary<String, String>> GenerateValidContracts()
        {
            view_User user = System.Web.HttpContext.Current.GetUser();
            List<view_Customer> customers;
            if (user.User_level > 1) 
                customers = view_Customer.getAllCustomers(user.Sign);
            else
                customers = view_Customer.getAllCustomers();

            List<Dictionary<String, String>> rows = new List<Dictionary<String, String>>();
            foreach (view_Customer customer in customers)
            {
                Dictionary<String, String> dict = new Dictionary<String, String>();
                int amountValidContracts = 0;
                List<view_Contract> contracts = view_Contract.GetContracts(customer.Customer);
                bool noValidContract = true;
                foreach (view_Contract contract in contracts)
                {
                    if (contract.Status == "Giltigt" || contract.Status == "Sänt" && user.IfSameArea(contract.Area))
                    {
                        noValidContract = false;
                    }
                }

                if (noValidContract)
                {
                    dict.Add("customer", customer.Customer);
                    dict.Add("customer_type", customer.Customer_type);
                    dict.Add("county", customer.GetCounty().ToString());
                    dict.Add("address", customer.Address);
                    dict.Add("zip_code", customer.Zip_code);
                    dict.Add("city", customer.City);
                    dict.Add("it_manager", customer.IT_manager);
                    dict.Add("it_manager_email", customer.IT_manager_email);
                    rows.Add(dict);
                }
            }
            return rows;
        }
    }
}