using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TietoCRM.Models
{
    public class view_CustomerProductGrowthSearchCriterias : SQLBaseClass
    {
        int _id;
        public int _ID { get; set; }

        string name;
        public string Name { get; set; }

        string customers;
        public string Customers { get; set; }

        string modules;
        public string Modules { get; set; }

        DateTime start;
        public DateTime Start { get; set; }

        DateTime stop;
        public DateTime Stop { get; set; }

        DateTime created;
        public DateTime Created { get; set; }

        string createdby;
        public string CreatedBy { get; set; }

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

                String query = "SELECT [ID], Name, Customers, Modules, Start, Stop, Created, CreatedBy FROM " + databasePrefix + "CustomerProductGrowthSearchCriterias ";

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