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
            List<Dictionary<String, object>> contracts = this.GenerateSentContracts();

            String sortDirection = Request["sort"];
            String sortKey = Request["prop"];

            ViewData.Add("Contracts", (new SortedByColumnCollection(contracts, sortDirection, sortKey)).Collection);

            this.ViewData["Title"] = "Sent Contracts Report";

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"\"";
            pdf.RotativaOptions.CustomSwitches += " --header-center \"Sent Contracts Report\"";

            return pdf;


        }

        public String SentContracts()
        {
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(this.GenerateSentContracts()) + "}";
        }

        public List<Dictionary<String, object>> GenerateSentContracts()
        {
            decimal? totalMaintenance = 0;
            decimal? totalLicense = 0;

            List<view_Customer> customers;
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                customers = view_Customer.getAllCustomers(System.Web.HttpContext.Current.GetUser().Sign);
            else
                customers = view_Customer.getAllCustomers();

            List<view_Contract> contracts = view_Contract.GetContracts();
            view_User user = System.Web.HttpContext.Current.GetUser();
            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            foreach (view_Contract contract in contracts)
            {
                List<view_Customer> t = customers.Where(c => c.Customer == contract.Customer).ToList();
                view_Customer customer = null;
                if (t.Count > 0)
                    customer = t.First();

                if (contract.Status == "Sänt" && user.IfSameArea(contract.Area) && customer != null)
                {
                    Dictionary<String, object> dict = new Dictionary<String, object>();
                    dict.Add("customer", contract.Customer);
                    dict.Add("customer_type", customer.Customer_type);
                    dict.Add("representative", customer.GetReprensentativesAsString());
                    dict.Add("contact_person", contract.Contact_person);
                    dict.Add("contract_id", contract.Contract_id);
                    dict.Add("title", contract.Title);
                    dict.Add("contract_type", contract.Contract_type);
                    dict.Add("totalMaintenance", contract.ContractMaintenanceSum());
                    dict.Add("totalLicense", contract.ContractLicenseSum());
                    dict.Add("totalService", contract.ContractServiceSum());
                    totalMaintenance += contract.ContractMaintenanceSum();
                    rows.Add(dict);
                }
            }
            ViewData.Add("totalMaintenance", totalMaintenance.ToString());
            return rows;
        }
        public List<Dictionary<String, object>> GenerateSentContracts(String User)
        {
            decimal? totalMaintenance = 0;
            decimal? totalLicense = 0;

            List<view_Customer> customers;
            if (System.Web.HttpContext.Current.GetUser().User_level > 1)
                customers = view_Customer.getAllCustomers(System.Web.HttpContext.Current.GetUser().Sign);
            else
                customers = view_Customer.getAllCustomers();

            List<view_Contract> contracts = view_Contract.GetContracts();
            view_User user = System.Web.HttpContext.Current.GetUser();
            List<Dictionary<String, object>> rows = new List<Dictionary<String, object>>();
            foreach (view_Contract contract in contracts)
            {
                List<view_Customer> t = customers.Where(c => c.Customer == contract.Customer).ToList();
                view_Customer customer = null;
                if (t.Count > 0)
                    customer = t.First();

                if (contract.Status == "Sänt" && user.IfSameArea(contract.Area) && customer != null)
                {
                    Dictionary<String, object> dict = new Dictionary<String, object>();
                    dict.Add("customer", contract.Customer);
                    dict.Add("customer_type", customer.Customer_type);
                    dict.Add("representative", customer.GetReprensentativesAsString());
                    dict.Add("contact_person", contract.Contact_person);
                    dict.Add("contract_id", contract.Contract_id);
                    dict.Add("title", contract.Title);
                    dict.Add("contract_type", contract.Contract_type);
                    dict.Add("totalMaintenance", contract.ContractMaintenanceSum());
                    dict.Add("totalLicense", contract.ContractLicenseSum());
                    dict.Add("totalService", contract.ContractServiceSum());
                    totalMaintenance += contract.ContractMaintenanceSum();
                    rows.Add(dict);
                }
            }
            ViewData.Add("totalMaintenance", totalMaintenance.ToString());
            return rows;
        }
    }
}