using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

using System.Data;

namespace TietoCRM.Controllers
{
    public class TariffController : Controller
    {
        // GET: Tariff
        public ActionResult Index()
        {
            this.createCookie();

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Tariff).GetProperties());
            this.ViewData.Add("ControllerName", "tariff");
            this.ViewData.Add("AjaxUrl", "/Tariff/TariffJsonData/");
            this.ViewData.Add("TargetUrl", "/Tariff/Data/");
            this.ViewData.Add("InsertUrl", "/Tariff/Insert/");
            this.ViewData.Add("DeleteUrl", "/Tariff/Delete/");
            this.ViewData.Add("PrimaryKey", "SSMA_timestamp");

            List<view_Tariff> tariffs = view_Tariff.getAllTariff();

            this.ViewData.Add("InhabitantLevel", tariffs.Select(t => t.Inhabitant_level).Distinct().ToList());
            this.ViewData.Add("PriceCategory", tariffs.Select(t => t.Price_category).Distinct().ToList());

            this.ViewData["Title"] = "Tariff";
           
            GlobalVariables.MostVisitedSites = this.getMostVisitedSite();
            return View();
        }

        protected HashSet<HttpCookie> getMostVisitedSite()
        {
            HashSet<HttpCookie> l = new HashSet<HttpCookie>();
            foreach (String name in this.Request.Cookies)
            {
                HttpCookie c = this.Request.Cookies.Get(name);
                l.Add(c);
            }
            return new HashSet<HttpCookie>(l.OrderByDescending(o => o.Value).ToArray());
        }

        public void createCookie()
        {
            if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("Tariff"))
            {
                List<String> l = new List<String>();
                l.Add("Tariff");
                l.Add("Customer");
                l.Add("Module");

                foreach (String name in l)
                {
                    HttpCookie cookie = new HttpCookie(name);
                    cookie.Value = Convert.ToString(0);
                    cookie.Expires = DateTime.Parse("2035-10-10 15:15");

                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                }
            }
            else
            {
                int value = Convert.ToInt32(this.ControllerContext.HttpContext.Request.Cookies.Get("Tariff").Value);
                value++;
                this.ControllerContext.HttpContext.Response.Cookies.Get("Tariff").Value = Convert.ToString(value);
            }
        }

        public String TariffJsonData()
        {
            this.Response.ContentType = "text/plain";
            List<view_Tariff> l = view_Tariff.getAllTariff();
            DateTime a = (new DateTime(1970,1,1,1,0,0,0)).AddMilliseconds(1420066800000);
            String s = a.ToString("yyyy-MM-dd");
            String jsonData = "{\"data\":" + (new JavaScriptSerializer()).Serialize(l) + "}";
            return Regex.Replace(jsonData, @"\\\/Date\(([0-9]+)\)\\\/", m =>
            {
                DateTime dt = new DateTime(1970, 1, 1, 1, 0, 0, 0);
                dt = dt.AddMilliseconds(Convert.ToDouble(m.Groups[1].Value));
                return dt.ToString("yyyy-MM-dd");
            });
        }

        public String SaveTariff()
        {
            try
            {
                String SSMA_timestamp = Request.Form["ssma_timestamp"];
                String json = Request.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_Tariff tariff = new view_Tariff();
                tariff.Select("SSMA_timestamp = " + SSMA_timestamp);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if(variable.Key == "Valid_through")
                    {
                        if(variable.Value.ToString() == "")
                            tariff.SetValue(variable.Key, null);
                        else
                            tariff.SetValue(variable.Key, variable.Value);
                    }
                    else
                        tariff.SetValue(variable.Key, variable.Value);
                }

                tariff.Update("SSMA_timestamp = " + SSMA_timestamp);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertTariff()
        {
            try
            {
                String json = Request.Form["json"];
                view_Tariff a = null;
                try
                {
                    a = (view_Tariff)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Tariff));
                }
                catch (Exception e)
                {
                    return "0";
                }

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeleteTariff()
        {
            try
            {
                String ssma_timestamp = Request.Form["ssma_timestamp"];
                view_Tariff a = new view_Tariff();
                //a.Select("Article_number = " + value);
                a.Delete("SSMA_timestamp = " + ssma_timestamp);
            }
            catch(Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}