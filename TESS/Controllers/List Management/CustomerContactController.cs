using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Extensions;

namespace TietoCRM.Controllers
{
    public class CustomerContactController : Controller
    {
        // GET: CustomerContact
        public ActionResult Index()
        {
            ViewData.Add("CustomerContacts", view_CustomerContact.getAllCustomerContacts());
            ViewData.Add("Customers", view_Customer.getCustomerNames(System.Web.HttpContext.Current.GetUser().Sign));
            ViewData.Add("Properties", typeof(view_CustomerContact).GetProperties());
            this.ViewData["Title"] = "Customer Contact";
            return View();
        }

        public String ContactData()
        {
            this.Response.ContentType = "text/plain";
            List<view_CustomerContact> l = view_CustomerContact.getAllCustomerContacts();
            foreach(view_CustomerContact contact in l)
            {
                contact.Customer = System.Web.HttpUtility.HtmlEncode(contact.Customer);
                contact.Contact_person = System.Web.HttpUtility.HtmlEncode(contact.Contact_person);
                contact.Email = System.Web.HttpUtility.HtmlEncode(contact.Email);
            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = 209715200;
            return "{\"data\":" + js.Serialize(l) + "}";
        }

        public String GetContact()
        {
            String name = Request.Form["name"];
            String customer = Request.Form["customer"];
            String email = Request.Form["email"];

            view_CustomerContact contact = new view_CustomerContact();
            contact.Select("Contact_person = '" + name + "' AND Customer = '" + customer + "' AND Email = '" + email + "'");

            contact.Customer = System.Web.HttpUtility.HtmlEncode(contact.Customer);
            contact.Contact_person = System.Web.HttpUtility.HtmlEncode(contact.Contact_person);
            contact.Email = System.Web.HttpUtility.HtmlEncode(contact.Email);

            return (new JavaScriptSerializer()).Serialize(contact);
        }

        public String SaveContact()
        {
            try
            {
                String oldEmail = Request.Form["oldEmail"];
                String oldName = Request.Unvalidated.Form["oldName"];
                String json = Request.Unvalidated.Form["json"];

                Dictionary<String, Object> variables = null;

                try
                {
                    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                }
                catch
                {
                    return "0";
                }

                view_CustomerContact contact = new view_CustomerContact();
                contact.Select("Contact_person = '" + oldName + "' AND Email = '" + oldEmail + "' AND Customer = '" + variables["Customer"] + "'");
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contact.SetValue(variable.Key, System.Web.HttpUtility.HtmlDecode(variable.Value.ToString()));
                    }

                    contact.Update("Contact_person = '" + System.Web.HttpUtility.HtmlDecode(oldName) + "' AND Email = '" + System.Web.HttpUtility.HtmlDecode(oldEmail) + "' AND Customer = '" + System.Web.HttpUtility.HtmlDecode(variables["Customer"].ToString()) + "'");

                    foreach (view_CustomerOffer co in view_CustomerOffer.getAllCustomerOffers(contact.Customer))
                    {
                        if (co.Contact_person == variables["Contact_person"])
                        {
                            co.SetContactPerson(contact.Contact_person);
                            co.Update("Customer = '" + co.Customer + "' AND Contact_person = '" + variables["Contact_person"] + "'");
                        }
                    }

                    foreach (view_Contract contract in view_Contract.GetContracts(contact.Customer))
                    {
                        if (contract.Contact_person == variables["Contact_person"])
                        {
                            contract.Contact_person = contact.Contact_person;
                            contract.Update("Customer = '" + contract.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
                        }
                    }
                }
                catch
                {
                    return "0";
                }


                return "1";
            }
            catch
            {
                return "-1";
            }
            
        }

        public String InsertContact()
        {
            try
            {
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

                view_CustomerContact contact = new view_CustomerContact();
                try
                {


                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contact.SetValue(variable.Key, variable.Value);
                    }

                    contact.Insert();
                }
                catch
                {
                    return "0";
                }

                return "1";
            }
            catch
            {
                return "-1";
            }
            
        }

        public String DeleteContact()
        {
            try
            {
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

                view_CustomerContact contact = new view_CustomerContact();
                try
                {
                    contact.Delete("Contact_person = '" + System.Web.HttpUtility.HtmlDecode(variables["Contact_person"].ToString()) + "' AND Email = '" + System.Web.HttpUtility.HtmlDecode(variables["Email"].ToString()) + "' AND Customer = '" + System.Web.HttpUtility.HtmlDecode(variables["Customer"].ToString()) + "'");
                }
                catch
                {
                    return "0";
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