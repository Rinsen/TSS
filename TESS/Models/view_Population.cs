using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Population : SQLBaseClass
    {
        private decimal id_pk;
        public decimal ID_PK { get { return id_pk; } set { id_pk = value; } }

        private decimal minpopulation;
        public decimal MinPopulation { get { return minpopulation; } set { minpopulation = value; } }

        private decimal maxpopulation;
        public decimal MaxPopulation { get { return maxpopulation; } set { maxpopulation = value; } }

        public view_Population()
            : base("Population")
        {
            //ctr
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Population> getAllPopulations()
        {
            List<view_Population> list = new List<view_Population>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], [MinPopulation], [MaxPopulation] FROM " + databasePrefix + "Population Order By 2";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Population k = new view_Population();
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