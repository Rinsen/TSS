using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Configuration;
using TietoCRM;
namespace TietoCRM.Models
{
    public static class GlobalVariables
    {
        public static void Initializer()
        {
            HttpContext.Current.Application["ApplicationName"] = ConfigurationManager.AppSettings["applicationName"];
           
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