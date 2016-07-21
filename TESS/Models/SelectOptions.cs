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

        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            Type[] returnList = assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
            returnList = returnList.Where(a => a.Name.StartsWith("view_")).ToArray();
            return returnList;
        }

        public static void RunInitAllModels()
        {
            Type[] allClasses = SelectOptionsBaseClass.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TietoCRM.Models");
            foreach (Type aClass in allClasses)
            {
                if (aClass.BaseType == typeof(SelectOptionsBaseClass))
                {
                    SelectOptionsBaseClass tempClass = (SelectOptionsBaseClass)Activator.CreateInstance(aClass, true);
                    tempClass.initTable();
                }
            }
        }
    }
}