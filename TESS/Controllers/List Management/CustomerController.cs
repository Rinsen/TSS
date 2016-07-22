using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using TietoCRM.Models;



namespace TietoCRM.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index()
        {
            this.createCookie();
            string amount = this.Request.QueryString["amount"];

            if (amount == "" || amount == null)
                amount = "0";

            this.ViewData.Add("Properties", typeof(TietoCRM.Models.view_Customer).GetProperties());

            String select = "[";
            foreach (view_User a in view_User.getAllUsers())
            {
                select += "[\"" + a.Sign + "\",\"" + a.Name + "\"]" + ",";
            }
            select = select.Remove(select.Length - 1);
            select += "]";

            this.ViewData.Add("Select", select);
            this.ViewData.Add("ControllerName", "customer");
            this.ViewData.Add("AjaxUrl", "/Customer/CustomerJsonData/");
            this.ViewData.Add("TargetUrl", "/Customer/Data/");
            this.ViewData.Add("InsertUrl", "/Customer/Insert/");
            this.ViewData.Add("DeleteUrl", "/Customer/Delete/");
            this.ViewData.Add("PrimaryKey", "Customer");
            this.ViewData.Add("Representatives", view_User.getAllUsers());
            this.ViewData.Add("Population", view_Population.getAllPopulations());
            this.ViewData["Title"] = "Customer";
            //this.ViewBag.Tile = "Customer";
            
            

            

            GlobalVariables.MostVisitedSites = this.getMostVisitedSite();

            return View();
        }


        public void createCookie()
        {
            if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("Customer"))
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
                int value = Convert.ToInt32(this.ControllerContext.HttpContext.Request.Cookies.Get("Customer").Value);
                value++;
                this.ControllerContext.HttpContext.Response.Cookies.Get("Customer").Value = Convert.ToString(value);
            }
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

        public String CustomerJsonData()
        {
            this.Response.ContentType = "text/plain";
            List<view_Customer> l = view_Customer.getAllCustomers();
            List<Dictionary<String, Object>> list = new List<Dictionary<String, Object>>();
            SelectOptions<view_Customer> selectOption = new SelectOptions<view_Customer>();

            foreach(view_Customer customer in l)
            {
                customer.Customer = System.Web.HttpUtility.HtmlEncode(customer.Customer);
                Dictionary<String, Object> dic = new Dictionary<String, Object>();
                foreach (PropertyInfo info in typeof(view_Customer).GetProperties())
                {
                    if(info.Name != "_ID" && info.Name != "Representative" && info.Name != "SSMA_timestamp")
                    {
                        if(info.Name != "County")
                            dic.Add(info.Name, info.GetValue(customer));
                        else
                            dic.Add(info.Name, customer.GetCounty(selectOption));

                    }
                        
                }
                list.Add(dic);
            }

            return "{\"data\":" + (new JavaScriptSerializer()).Serialize(list) + "}";
        }

        public String SaveCustomer()
        {
            try
            {
                // Customer to select from DB. This might change after update.
                String oldCustomer = Request.Form["oldCustomer"];
                String customerData = Request.Form["customerData"];


                Dictionary<String, Object> customerVariables = null;

                try
                {
                    customerVariables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(customerData, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                try
                {
                    view_Customer customer = new view_Customer();
                    customer.Select("Customer = '" + oldCustomer + "'");

                    foreach (KeyValuePair<String, object> customerVariable in customerVariables)
                    {
                        if (customerVariable.Key == "_Representatives")
                        {
                            foreach(String rep in ((System.Collections.ArrayList)customerVariable.Value))
                            {
                                customer._Representatives.Add(rep);
                            }
                            
                        }
                        else if (customerVariable.Key == "Customer" && customerVariable.Value.ToString() != oldCustomer)
                        {
                            customer.SetCustomer(Convert.ToString(customerVariable.Value));
                        }
                        else
                        {
                            customer.SetValue(customerVariable.Key, customerVariable.Value);
                        }
                       // customer.SetValue(customerVariable.Key, customerVariable.Value);
                    }
                    if (customer._Representatives.Count > 0)
                        customer.Update("Customer = '" + oldCustomer + "'");
                    else
                        return "-1";
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    // Return -1 on any possible arror
                    return "-1";
                }


                return "1";
            }
            catch
            {
                return "-1";
            }
            
        }

        public String InsertCustomer()
        {
            try
            {
                String customerData = Request.Form["customerData"];


                Dictionary<String, Object> customerVariables = null;

                try
                {
                    customerVariables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(customerData, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                try
                {
                    view_Customer customer = new view_Customer();

                    foreach (KeyValuePair<String, object> customerVariable in customerVariables)
                    {
                        if(customerVariable.Key != "_Representatives")
                            customer.SetValue(customerVariable.Key, customerVariable.Value);               
                    }

                    customer.Insert(((System.Collections.ArrayList)customerVariables["_Representatives"]).Cast<string>().ToList());
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    // Return -1 on any possible arror
                    return "-1";
                }
                return "1";
            }
            catch
            {
                return "-1";
            }
           
        }

        public String Data()
        {

            try
            {
                String json = Request.Form["object"];
                List<dynamic> map = null;
                try
                {
                    map = (List<dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(List<dynamic>));
                }
                catch (Exception e)
                {
                    return "0";
                }

                foreach (Dictionary<String, object> d in map)
                {
                    view_Customer customer = new view_Customer();
                    if (d.ContainsKey("insert"))
                    {
                        foreach (KeyValuePair<String, object> entry in d)
                        {
                            if (entry.Key != "insert")
                                customer.SetValue(entry.Key, entry.Value);
                        }
                        customer.Insert();
                    }
                    else
                    {
                        String compareValue = Convert.ToString(d["primaryKey"]);
                        customer.Select("Customer = " + "'" + compareValue + "'");
                        foreach (KeyValuePair<String, object> entry in d)
                        {
                            if (entry.Key == "Representative")
                            {
                                customer.SetRepresentative(Convert.ToString(d["Representative"]));
                            }
                            else if(entry.Key == "Customer" && entry.Value != compareValue)
                            {
                                customer.SetCustomer(Convert.ToString(entry.Value));
                            }  
                            else
                            {
                                customer.SetValue(entry.Key, entry.Value);
                            }  
                        }
                        customer.Update("Customer = " + "'" + compareValue + "'");
                        
                    }
                }
                return "1";
            }
            catch (Exception e)
            {
                return "-1";
            }
        }

        public String Insert()
        {
            try
            {
                String json = Request.Form["json"];
                view_Customer a = null;
                try
                {
                    a = (view_Customer)(new JavaScriptSerializer()).Deserialize(json, typeof(view_Customer));
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

        public String Delete()
        {
            try
            {
                String value = Request.Form["primaryKey"];
                view_Customer a = new view_Customer();
                //a.Select("Article_number = " + value);
                a.Delete("Customer = '" + value + "'");
            }
            catch (Exception e)
            {
                return "-1";
            }
            return "1";
        }


    }
}