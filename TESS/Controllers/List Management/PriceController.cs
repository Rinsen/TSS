using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;

namespace TietoCRM.Controllers.List_Management
{
    public class PriceController : Controller
    {
        // GET: Price
        public ActionResult Index()
        {
            ViewBag.Properties = typeof(TietoCRM.Models.view_Price).GetProperties();
            //this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Price).GetProperties());
            this.ViewData["title"] = "Price";

            return View();
        }

        public String PriceJsonData()
        {
            this.Response.ContentType = "text/plain";
            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(view_Price.getAllPrices()) + "}";
        }

        public String SavePrice()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
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

                view_Price price = new view_Price();
                price.Select("ID_PK = " + id_pk);

                foreach (KeyValuePair<String, object> variable in variables)
                {
                    if (variable.Key != "id_pk")
                        price.SetValue(variable.Key, variable.Value);
                }

                price.Update("ID_PK = " + id_pk);

                return "1";
            }
            catch
            {
                return "-1";
            }
        }

        public String InsertPrice()
        {
            try
            {
                String json = Request.Form["json"];
                view_Price a = null;
                try
                {
                    a = (view_Price)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Price));
                }
                catch (Exception e)
                {
                    return "0";
                }

                List<view_Price> services = view_Price.getAllPrices();

                a.Insert();

                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String DeletePrice()
        {
            try
            {
                String id_pk = Request.Form["id_pk"];
                view_Price a = new view_Price();
                //a.Select("Article_number = " + value);
                a.Delete("ID_PK = " + id_pk);
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }
    }
}