using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TietoCRM.Models;
using TietoCRM.Extensions;
using System.IO;
using System.Reflection;

namespace TietoCRM.Extensions
{
    public static class Extensions
    {
        public static view_User GetUser(this HttpContext current)
        {
            current.Session["__User"] = new view_User();
            ((view_User)(current.Session["__User"])).Select("windows_user='" + System.Security.Principal.WindowsPrincipal.Current.Identity.Name + "'");
            return (view_User)current.Session["__User"];
        }
        public static String GetUserRedirectUrl(this HttpContext current)
        {
            return current != null ? (String)current.Session["__UserRedirectUrl"] : null;
        }

        public static void UpdateUser(this HttpContext current, view_User user)
        {
            current.Session["__User"] = user;
        }
    }
}

namespace TietoCRM
{
    
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            TietoCRM.Models.GlobalVariables.Initializer();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e) {

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            view_User CurrentUser = new view_User();
            String name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (!HttpContext.Current.Items.Contains("__User") && Request.Url.AbsolutePath.StartsWith("/Access/Denied") && Request.Url.AbsolutePath.StartsWith("/Access/Login"))
            {
                if (CurrentUser.Select(@"windows_user = '" + name + "'"))
                {
                    HttpContext.Current.Session.Add("__User", CurrentUser);
                }
                else
                {
                    HttpContext.Current.Session.Abandon();
                    HttpContext.Current.Session.Clear();
                    //HttpContext.Current.Session.Add("__UserRedirectUrl", Request.Url.AbsoluteUri);
                    base.Response.StatusCode = 0x191;
                    Response.Redirect("/Access/Denied/");
                    
                }
            }
            else if (!CurrentUser.Select(@"windows_user = '" + name + "'") && !Request.Url.AbsolutePath.StartsWith("/Access/Denied") && !Request.Url.AbsolutePath.StartsWith("/Access/Login"))
            {
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Session.Clear();
                base.Response.StatusCode = 0x191;
                Response.Redirect("/Access/Login/");
            }
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            view_Exception.UploadException(exception);

            Server.ClearError();
            if (!HttpContext.Current.Items.Contains("__User") && !Request.Url.AbsolutePath.StartsWith("/Access/Denied")
                && !Request.Url.AbsolutePath.StartsWith("/Access/Login") && HttpContext.Current.Session != null)
                Response.Redirect("/Error/Index");
        }


    }
}
