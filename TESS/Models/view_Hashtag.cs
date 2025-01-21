using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace TietoCRM.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class view_Hashtag : SQLBaseClass
    {
        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Document_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SQL_table { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Hashtag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public view_Hashtag()
            : base("Hashtag")
        {
            //ctr
        }

        /// <summary>
        /// Gets all main contract templates.
        /// </summary>
        /// <returns>A lsit of main contract templates.</returns>
        public static List<view_Hashtag> getAllHashTags()
        {
            List<view_Hashtag> list = new List<view_Hashtag>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = "SELECT ID, Document_id, SQL_table, Hashtag " +
                            "FROM " + databasePrefix + "Hashtag H1 " +
                            "WHERE H1.ID = (" +
                            "   SELECT TOP 1 H2.ID " +
                            "    FROM " + databasePrefix + "Hashtag H2 " +
                            "    WHERE H2.Hashtag = H1.Hashtag" +
                            ")" +
                            " ORDER BY Hashtag";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Hashtag k = new view_Hashtag();
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