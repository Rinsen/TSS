using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Service : SQLBaseClass
    {
        private int code;
        public int Code { get { return code; } set { code = value; } }

        private String description;
        public String Description { get { return description; } set { description = value; } }

        private decimal? price;
        public decimal? Price { get { return price; } set { price = value; } }

        public view_Service()
            : base("Service")
        {
            //ctr
        }

        /// <summary>
        /// Gets all services.
        /// </summary>
        /// <returns>A list of all services.</returns>
        public static List<view_Service> getAllServices()
        {
            List<view_Service> list = new List<view_Service>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Code ,Description ,Price FROM " + databasePrefix + "Service";

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 1;
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_Service t = new view_Service();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }
                            list.Add(t);
                            count++;
                        }
                    }
                }


            }
            return list;
        }
    }

}