using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Configuration;
using TietoCRM;
using TietoCRM.Extensions;
using System.Security;

namespace TietoCRM.Models
{
    public static class GlobalVariables
    {
        /// <summary>
        /// User Level types 
        /// </summary>
        public enum UserLevel
        {
            /// <summary>
            /// Can see everything 
            /// </summary>
            Supervisor = 1,
            /// <summary>
            /// Can only see things within same area 
            /// </summary>
            Salesperson = 2,

            /// <summary>
            /// Can see everything except Users, Contracts and Offers
            /// </summary>
            ProductOwner = 3
        }

        public static void Initializer()
        {
            HttpContext.Current.Application["ApplicationName"] = ConfigurationManager.AppSettings["applicationName"];
            FeatureServiceProxy.ServiceUri = ConfigurationManager.AppSettings["ServiceUrl"];
        }

        public static void checkIfAuthorized(String site)
        {
            if(!isAuthorized(site))
                throw new SecurityException("Invalid user level. The user does not have premission to be enter this area!");
        }

        public static bool isAuthorized(String site)
        {
            view_User user = new view_User();
            user.Select("windows_user='" + WindowsIdentity.GetCurrent().Name + "'");
            System.Web.HttpContext.Current.UpdateUser(user);
            if (user.User_level > 2 && (site == "CustomerContract" || site == "CustomerOffer" || site == "Users"))
                return false;
            else
                return true;
        }

        public static bool isAuthorized(UserLevel level)
        {
            view_User user = new view_User();
            user.Select("windows_user='" + WindowsIdentity.GetCurrent().Name + "'");
            System.Web.HttpContext.Current.UpdateUser(user);
            switch (level)
            {
                case UserLevel.Supervisor:
                    if (user.User_level <= 1)
                        return true;
                    break;
                case UserLevel.Salesperson:
                    if (user.User_level == 2)
                        return true;
                    break;
                case UserLevel.ProductOwner:
                    if (user.User_level == 3)
                        return true;
                    break;
                default:
                    return false;
            }
            return false;
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
                    return "TietoEVRY, Welfare Nordic ";
                else
                    return "TietoEVRY, Welfare Nordic ";
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