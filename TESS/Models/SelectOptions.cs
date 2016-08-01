using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;


namespace TietoCRM.Models
{
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

    public class SelectOptions<T> where T : SQLBaseClass
    {
        private Dictionary<String, List<SelectOption>> options;
        public ReadOnlyDictionary<String, List<SelectOption>> Options
        {
            get
            {
                return new ReadOnlyDictionary<String, List<SelectOption>>(this.options);
            }
        }
        
        public String GetValue(String prop, String value)
        {
            return this.options[prop].Find(d => d.Value == value).Text;
        }

        public ReadOnlyCollection<SelectOption> GetOptions(String prop)
        {
            return this.options[prop].AsReadOnly();
        }

        public struct SelectOption
        {
            public String Value;
            public String Text;
        }

        public SelectOptions()
        {
            UpdateData();
        }

        public void UpdateData()
        {
            this.options = this.GetSelectOptions();
        }

        public virtual void initTable()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String query = @"   IF NOT EXISTS 
                                    (   SELECT  1
                                        FROM    " + "view_" + @"SelectOption 
                                        WHERE   Model = @Model 
                                        AND     Property = @Property
                                        AND     Value = @Value
                                    )
                                    BEGIN
                                        INSERT INTO " + "view_" + @"SelectOption (Model, Property, Value, Text)
                                        VALUES(@Model, @Property, @Value, @Text)
                                    END";

                SqlCommand command = new SqlCommand(query, connection);
                command.Prepare();
                command.Parameters.AddWithValue("@Model", null);
                command.Parameters.AddWithValue("@Property", "Property");
                command.Parameters.AddWithValue("@Value", null);
                command.Parameters.AddWithValue("@Text", null);
                foreach (PropertyInfo pi in typeof(T).GetProperties())
                {
                    if (!pi.Name.StartsWith("_") && pi.Name != "SSMA_timestamp" && pi.Name != "ID_PK")
                    {
                        command.Parameters[0].Value = typeof(T).Name.ToString();
                        command.Parameters[2].Value = pi.Name.ToString();
                        command.Parameters[3].Value = AddSpacesToSentence(pi.Name.ToString()).Replace("_", " ");
                        command.ExecuteNonQuery();
                    }
                }

                // Make class available as a select option
                command.Parameters[0].Value = "view_SelectOption";
                command.Parameters[1].Value = "Model";
                command.Parameters[2].Value = typeof(T).Name;
                String text = typeof(T).Name.Replace("view_", "");
                command.Parameters[3].Value = AddSpacesToSentence(text);
                command.ExecuteNonQuery();
            }
        }
        public Dictionary<String, List<SelectOption>> GetSelectOptions()
        {
            Dictionary<String, List<SelectOption>> returnDic = new Dictionary<String, List<SelectOption>>();
            String model = typeof(T).Name.ToString();
            List<view_SelectOption> allSelectOptions = view_SelectOption.getAllSelectOptionsWhere("Model = '" + model + "'").OrderBy(s => s.Property).ToList();
            foreach (view_SelectOption so in allSelectOptions)
            {
                if(!returnDic.ContainsKey(so.Property))
                {
                    returnDic.Add(so.Property, new List<SelectOption>());
                }

                SelectOption sel;
                sel.Value = so.Value;
                sel.Text = so.Text;
                returnDic[so.Property].Add(sel);
            }

            return returnDic;
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