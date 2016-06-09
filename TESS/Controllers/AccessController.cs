using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TietoCRM.Extensions;

namespace TietoCRM.Controllers
{
    public class AccessController : Controller
    {
        // GET: Access
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        public ActionResult Login()
        {     
            TietoCRM.Models.view_User user = new TietoCRM.Models.view_User();
            if (this.User.Identity.IsAuthenticated && user.Select("Windows_user = '" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "'"))
            {
                System.Web.HttpContext.Current.Session.Add("__User", user);
                return RedirectToAction("Index", "Home");
            }

            base.Response.AppendHeader("Connection", "close");
            base.Response.StatusCode = 0x191;
            base.Response.Clear();
            //should probably do a redirect here to the unauthorized/failed login page
            //if you know how to do this, please tap it on the comments below
            base.Response.End();
            return RedirectToAction("Denied");        
        }
        public ActionResult Denied()
        {
            return View();
        }
        public ActionResult Destory()
        {
            System.Web.HttpContext.Current.Session.Clear();
            System.Web.HttpContext.Current.Session.RemoveAll();
            System.Web.HttpContext.Current.Session.Abandon();
            return RedirectToAction("Index", "Home");  
        }
    }
}