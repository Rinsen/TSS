using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TietoCRM.Models;

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
            foreach (T item in collection)
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
