using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TietoCRM.Models
{
    public class view_CustomerProductGrowthSearchCriterias : SQLBaseClass
    {
        /// <summary>
        /// Identity of table
        /// </summary>
        public int ID { get; set; }

        public string Name { get; set; }

        public string Customers { get; set; }

        public string Modules { get; set; }

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        /// <summary>
        /// View against table CustomerProductGrowthSearchCriterias
        /// </summary>
        public view_CustomerProductGrowthSearchCriterias() : base("CustomerProductGrowthSearchCriterias")
        {

        }

        /// <summary>
        /// Inserts this object into the SQL server. And automatically inserts the Created and Updated field.
        /// </summary>
        public override int Insert()
        {
            this.Created = DateTime.Now;
            return base.Insert();
        }

        /// <summary>
        /// Gets all information.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_CustomerProductGrowthSearchCriterias> getAllSearchCriterias()
        {
            List<view_CustomerProductGrowthSearchCriterias> list = new List<view_CustomerProductGrowthSearchCriterias>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT [ID], Name, Customers, Modules, Start, Stop, Created, CreatedBy FROM " + databasePrefix + "CustomerProductGrowthSearchCriterias ";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_CustomerProductGrowthSearchCriterias k = new view_CustomerProductGrowthSearchCriterias();
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            k.SetValue(k.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(k);
                    }
                }
            }

            return list;
        }
    }
}