using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;
using TietoCRM.Extensions;
using System.IO;
using System.Text;

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
            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = 209715200;
            return "{\"data\":" + js.Serialize(l) + "}";
        }

        public String GetContact()
        {
            String id = Request.Form["id"];

            view_CustomerContact contact = new view_CustomerContact();
            contact.Select("ID=" + id);

            return (new JavaScriptSerializer()).Serialize(contact);
        }

        public String SaveContact()
        {
            try
            {
                String id = Request.Form["id"];
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
                contact.Select("ID="+id);
                try
                {
                    foreach (KeyValuePair<String, object> variable in variables)
                    {
                        contact.SetValue(variable.Key, System.Web.HttpUtility.HtmlDecode(variable.Value.ToString()));
                    }

                    contact.Update("ID=" + id);

                    foreach (view_CustomerOffer co in view_CustomerOffer.getAllCustomerOffers(contact.Customer))
                    {
                        if (co.Contact_person == variables["Contact_person"])
                        {
                            co.SetContactPerson(contact.Contact_person);
                            co.Update("Offer_number="+co._Offer_number);
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
                catch (Exception e)
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

        public ActionResult ExportContact()
        {
            String id = Request["id"];

            view_CustomerContact contact = new view_CustomerContact();
            contact.Select("ID=" + id);

            byte[] byteArray = Encoding.GetEncoding("iso-8859-1").GetBytes(contact.ParseTovCard());

            return File(byteArray, "text/x-vcard", contact.Contact_person.Replace(" ","_") + "_as_vcard.vcf");

        }

        public String DeleteContact()
        {
            try
            {
                String id = Request["id"];

                Dictionary<String, Object> variables = null;

                //try
                //{
                //    variables = (Dictionary<String, dynamic>)(new JavaScriptSerializer()).Deserialize(json, typeof(Dictionary<String, dynamic>));
                //}
                //catch
                //{
                //    return "0";
                //}

                view_CustomerContact contact = new view_CustomerContact();
                try
                {
                    //contact.Delete("Contact_person = '" + System.Web.HttpUtility.HtmlDecode(variables["Contact_person"].ToString()) + "' AND Email = '" + System.Web.HttpUtility.HtmlDecode(variables["Email"].ToString()) + "' AND Customer = '" + System.Web.HttpUtility.HtmlDecode(variables["Customer"].ToString()) + "'");
                    contact.Delete("ID=" + id);
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