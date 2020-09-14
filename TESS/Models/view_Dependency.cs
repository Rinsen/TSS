using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Dependency : SQLBaseClass
    {
        private int article_number_pk;
        public int Article_number { get { return article_number_pk; } set { article_number_pk = value; } }

        private int service_number_pk;
        public int Service_number { get { return service_number_pk; } set { service_number_pk = value; } }

        public view_Dependency()
            : base("Dependency")
        {
            //ctr
        }


        /// <summary>
        /// Gets all dependencies.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Dependency> getAllDependencies()
        {
            List<view_Dependency> list = new List<view_Dependency>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT Article_number, Service_number FROM " + databasePrefix + "Article_Service_Dependency Order By 2";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Dependency k = new view_Dependency();
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