using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TietoCRM.Extensions;

namespace TietoCRM.Models
{
    public class view_HashtagReport : SQLBaseClass
    {
        public int Offer_number { get; set; }
        public string Offer_customer { get; set; }
        public string Offer_title { get; set; }
        public DateTime Offer_created { get; set; }
        public DateTime Offer_valid { get; set; }
        public string Offer_sign { get; set; }
        public string Contract_id { get; set; }
        public string Contract_customer { get; set; }
        public string Contract_title { get; set; }
        public string Contract_sign { get; set; }
        public string Hashtag { get; set; }

        public view_HashtagReport()
            : base("HashtagReport")
        {
            //ctr
        }

        /// <summary>
        /// Gets all main contract templates.
        /// </summary>
        /// <returns>A lsit of main contract templates.</returns>
        public static List<view_HashtagReport> getHashTagReportRows(List<string> users, bool isContract, bool isOffer)
        {
            List<view_HashtagReport> list = new List<view_HashtagReport>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var first = true;

                var query = "SELECT Offer_number, Offer_customer, Offer_title, Offer_created, Offer_valid, Offer_sign, Contract_id, Contract_customer, Contract_title, Contract_sign, Hashtag FROM " + databasePrefix + "HashtagReport";

                if(users != null && users.Count > 0)
                {
                    first = false;
                    query += " WHERE (Contract_sign IN (" + string.Join(", ", users.Select(s => $"'{s}'")) + ") OR Offer_sign IN (" + string.Join(", ", users.Select(s => $"'{s}'")) + "))";
                }

                if (!isContract && !isOffer)
                {
                    if (first)
                    {
                        query += " WHERE Contract_id IS NULL AND Offer_number IS NULL";
                    }
                    else
                    {
                        query += " AND Contract_id IS NULL AND Offer_number IS NULL";
                    }
                }
                else
                {
                    if (!isContract)
                    {
                        if (first)
                        {
                            query += " WHERE Contract_id IS NULL";
                        }
                        else
                        {
                            query += " AND Contract_id IS NULL";
                        }
                    }

                    if (!isOffer)
                    {
                        if (first)
                        {
                            query += " WHERE Offer_number IS NULL";
                        }
                        else
                        {
                            query += " AND Offer_number IS NULL";
                        }
                    }
                }

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_HashtagReport k = new view_HashtagReport();
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