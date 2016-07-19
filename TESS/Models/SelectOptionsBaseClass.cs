using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TietoCRM.Models
{
    public abstract class SelectOptionsBaseClass : SQLBaseClass
    {
        public struct SelectOption
        {
            public String Value;
            public String Text;
        }
        public SelectOptionsBaseClass(String table) : base(table)
        {
            initTable();
        }
        protected virtual void initTable()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = @"   IF NOT EXISTS 
                                    (   SELECT  1
                                        FROM    " + databasePrefix + @"SelectOption 
                                        WHERE   Model = @Model 
                                        AND     Property = @Property
                                        AND     Value = @Value
                                    )
                                    BEGIN
                                        INSERT INTO " + databasePrefix + @"SelectOption (Model, Property, Value, Text)
                                        VALUES(@Model, @Property, @Value, @Text)
                                    END";

                foreach(PropertyInfo pi in this.GetType().GetProperties())
                {
                    if(!pi.Name.StartsWith("_"))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Prepare();
                        command.Parameters.AddWithValue("@Model", this.GetType().Name.ToString());
                        command.Parameters.AddWithValue("@Property", "Property");
                        command.Parameters.AddWithValue("@Value", pi.Name.ToString());
                        command.Parameters.AddWithValue("@Text", pi.Name.ToString());
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<SelectOption> GetSelectOptions(String propertyName)
        {
            List<SelectOption> returnList = new List<SelectOption>();
            String model = this.GetType().Name.ToString();
            List<view_SelectOption> allSelectOptions = view_SelectOption.getAllSelectOptionsWhere("Model = '" + model + "' AND Property = '" + propertyName + "'");
            foreach(view_SelectOption so in allSelectOptions)
            {
                SelectOption sel;
                sel.Value = so.Value;
                sel.Text = so.Text;
                returnList.Add(sel);
            }

            return returnList;
        }
    }
}
