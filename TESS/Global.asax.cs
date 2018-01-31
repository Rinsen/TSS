using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TietoCRM.Models;

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
                    Response.Redirect("~/Access/Denied/");
                    
                }
            }
            else if (!CurrentUser.Select(@"windows_user = '" + name + "'") && !Request.Url.AbsolutePath.StartsWith("/Access/Denied") && !Request.Url.AbsolutePath.StartsWith("/Access/Login"))
            {
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Session.Clear();
                base.Response.StatusCode = 0x191;
                Response.Redirect("~/Access/Login/");
            }
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();

            view_Exception.UploadException(exception);

            Server.ClearError();
            if (!HttpContext.Current.Items.Contains("__User") && !Request.Url.AbsolutePath.StartsWith("/Access/Denied")
                && !Request.Url.AbsolutePath.StartsWith("/Access/Login") && HttpContext.Current.Session != null)
                Response.Redirect("~/Error/Index");
        }


    }
}
