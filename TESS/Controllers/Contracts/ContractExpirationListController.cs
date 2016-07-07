using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Models.Contract;
using System.Text.RegularExpressions;
using TietoCRM.Extensions;

namespace TietoCRM.Controllers.Contracts
{
    public class ContractExpirationListController : Controller
    {
        //
        // GET: /ContractExpirationList/
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(view_ContractExpirationList).GetProperties();
            this.ViewData["title"] = "Expiration List";

            view_User all = new view_User();
            List<view_User> userList = view_User.getAllUsers();
            userList.Add(all);
            ViewData.Add("Users", userList);

            return View();
        }

        public String JsonData()
        {
            String sign = Request.Form["sign"];

            this.Response.ContentType = "text/plain";
            view_User user = new view_User();
            user.Select("Sign=" + sign);
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_ContractExpirationList.GetContractExpirationList(sign).Where(c => user.IfSameArea(c.Area))) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 4, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }

        public String ExtendContracts()
        {
            try
            {
                List<dynamic> contracts = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(Request.Form["contracts"], typeof(List<dynamic>));

                foreach (dynamic contract in contracts)
                {
                    int ret = view_ContractExpirationList.ExtendContract(contract["Contract_id"], contract["Customer"]);
                }
                return "1";
            }
            catch
            {
                return "-1";
            }
        }
    }
}
