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
using System.Text;

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
    public static class ListExtensions
    {
        public static List<SelectOptions<view_SelectOption>.SelectOption> ToSelectOptionsList<T>(this List<T> collection)
        {
            List<SelectOptions<view_SelectOption>.SelectOption> returnList = new List<SelectOptions<view_SelectOption>.SelectOption>();
            foreach(T item in collection)
            {
                SelectOptions<view_SelectOption>.SelectOption so;
                so.Value = item.ToString();
                so.Text = AddSpacesToSentence(item.ToString().Replace("view_", ""));
                returnList.Add(so);
            }
            return returnList;
        }
        
        private static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
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
