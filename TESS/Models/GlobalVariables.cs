using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Configuration;
using TietoCRM;
using TietoCRM.Extensions;

namespace TietoCRM.Models
{
    public static class GlobalVariables
    {
        public static void Initializer()
        {
            HttpContext.Current.Application["ApplicationName"] = ConfigurationManager.AppSettings["applicationName"];
        }

        public static void checkIfAuthorized(String site)
        {
            if(!isAuthorized(site))
                throw new Exception("Invalid user level. You do not have premission to be here!");
        }

        public static bool isAuthorized(String site)
        {
            view_User user = new view_User();
            user.Select("Sign='" + System.Web.HttpContext.Current.GetUser().Sign + "'");
            System.Web.HttpContext.Current.UpdateUser(user);
            if (user.User_level > 2 && (site == "CustomerContract" || site == "CustomerOffer" || site == "Users"))
                return false;
            else
                return true;
        }

        // read-write variable
        public static string ApplicationName
        {
            get
            {
                return HttpContext.Current.Application["ApplicationName"] as string;
            }
            set
            {
                HttpContext.Current.Application["ApplicationName"] = value;
            }
        }
        public static string CompanyName
        {
            get
            {
                if (HttpContext.Current.Application["ApplicationName"].ToString().StartsWith("TESS"))
                    //return "Tieto Education AB"
                    return "Tieto Sweden Healthcare & Welfare AB";
                else
                    return "Tieto Sweden Healthcare & Welfare AB";
            }
            set
            {
                HttpContext.Current.Application["ApplicationName"] = value;
            }
        }
        private static view_User windowsUser;
        public static view_User WindowsUser
        {
            get
            {
                return windowsUser;
            }
            set
            {
                windowsUser = value;
            }
        }

        private static HashSet<HttpCookie> cookies;
        public static HashSet<HttpCookie> MostVisitedSites
        {
            get
            {
                return cookies;
            }
            set
            {
                List<String> validCookies = new List<String>();
                validCookies.Add("Tariff");
                validCookies.Add("Customer");
                validCookies.Add("Module");

                HashSet<HttpCookie> hs = new HashSet<HttpCookie>();

                foreach(HttpCookie c in value)
                {
                    if(validCookies.Contains(c.Name))
                    {
                        hs.Add(c);
                    }
                }

                cookies = hs;
            }
        }
    }


}