using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace TietoCRM.Models
{
    public class SelectOptions<T> where T : SQLBaseClass
    {
        public struct SelectOption
        {
            public String Value;
            public String Text;
        }

        public SelectOptions()
        {
        }

        public List<SelectOption> GetSelectOptions(String propertyName)
        {
            List<SelectOption> returnList = new List<SelectOption>();
            String model = typeof(T).Name.ToString();
            List<view_SelectOption> allSelectOptions = view_SelectOption.getAllSelectOptionsWhere("Model = '" + model + "' AND Property = '" + propertyName + "'");
            foreach (view_SelectOption so in allSelectOptions)
            {
                SelectOption sel;
                sel.Value = so.Value;
                sel.Text = so.Text;
                returnList.Add(sel);
            }

            return returnList;
        }

        private string AddSpacesToSentence(string text, bool preserveAcronyms = true)
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