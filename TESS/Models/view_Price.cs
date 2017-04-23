using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Price : SQLBaseClass
    {
        private decimal id_pk;
        public decimal ID_PK { get { return id_pk; } set { id_pk = value; } }

        private decimal level;
        public decimal Level { get { return level; } set { level = value; } }

        public view_Price()
            : base("Price")
        {
            //ctr
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Price> getAllPrices()
        {
            List<view_Price> list = new List<view_Price>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], [Level] FROM " + databasePrefix + "Price Order By [Level]";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Price k = new view_Price();
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
            //list.OrderBy(p => p.Level);
            return list;
        }
    }

}