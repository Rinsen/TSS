using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.Contracts
{
    public class ContractSupervisionController : Controller
    {
        // GET: ContractSupervision
        public ActionResult Index()
        {
            List<PropertyInfo> properties = typeof(view_Contract).GetProperties().Where(p => p.Name == "Customer" || p.Name == "Contract_id" || p.Name == "Valid_through" || p.Name == "Extension"
                || p.Name == "Term_of_notice" || p.Name == "Observation" || p.Name == "Expire").ToList();
            ViewData.Add("Properties", properties);
            this.ViewData["Title"] = "Contract Supervision";
            ViewData.Add("Users", view_User.getAllUsers());
            return View();
        }

        public ActionResult Pdf()
        {
            List<PropertyInfo> properties = typeof(view_Contract).GetProperties().Where(p => p.Name == "Customer" || p.Name == "Contract_id" || p.Name == "Valid_through" || p.Name == "Extension"
                || p.Name == "Term_of_notice" || p.Name == "Observation" || p.Name == "Expire").ToList();
            ViewData.Add("Properties", properties);
            ViewData.Add("Users", view_User.getAllUsers());

            this.ViewData["Title"] = "Contract Supervision";
            ViewData.Add("Contracts", view_Contract.GetValidContracts(Request["sign"]).OrderBy(c => c.Observation));

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            view_User user = new view_User();
            user.Select("Sign = '" + Request["sign"] + "'");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + DateTime.Now.ToString("yyyy-MM-dd") + "\" --header-left \"" + user.Name + "\"";
            return pdf;
        }

        public String GetData()
        {
            String sign = Request.Form["sign"];
            List<dynamic> contracts = view_Contract.GetValidContracts(sign).OrderBy(c => c.Observation).Select(c => new
                {
                    Customer = c.Customer,
                    Contract_id = c.Contract_id,
                    Valid_through = c.Valid_through,
                    Extension = c.getStringExtension(),
                    Term_of_notice = c.getStringTON(),
                    Observation = c.Observation,
                    Expire = c.Expire
                }).ToList<dynamic>();

            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Math.Abs(Int32.MaxValue);
            String jsonData = "{\"data\":" + jss.Serialize(contracts) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }
    }

}