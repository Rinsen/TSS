﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_OfferRow : SQLBaseClass
    {
        private int offer_number;
        public int Offer_number { get { return offer_number; } set { offer_number = value; } }

        private int article_number;
        public int Article_number { get { return article_number; } set { article_number = value; } }

        private decimal? license;
        public decimal? License { get { return license; } set { license = value; } }

        private decimal? maintenance;
        public decimal? Maintenance { get { return maintenance; } set { maintenance = value; } }

        private bool? include_status;
        public bool? Include_status { get { return include_status; } set { include_status = value; } }

        private int sortnr;
        public int Sortnr { get; set; }

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_OfferRow()
            : base("OfferRow")
        {
            //ctr
        }

        /// <summary>
        /// Compares offer rows on article number,
        /// </summary>
        /// <param name="obj">The other offer row</param>
        /// <returns>true/false depending on the compare result.</returns>
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_OfferRow p = obj as view_OfferRow;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Article_number == p.Article_number);
        }

        /// <summary>
        /// Gets all the offer rows with a specific offer number
        /// </summary>
        /// <param name="offerNumber">The offer number</param>
        /// <returns>A list of offer rows with a specific offer number.</returns>
        public static List<view_OfferRow> getAllOfferRows(String offerNumber)
        {
            List<view_OfferRow> list = new List<view_OfferRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT Offer_number, Article_number, License, 
                                        Maintenance, Include_status, sortnr, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp 
                                        FROM " + databasePrefix + "OfferRow WHERE " + "Offer_number = @offerNumber";

                command.Prepare();
                command.Parameters.AddWithValue("@offerNumber", offerNumber);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_OfferRow t = new view_OfferRow();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }
                            list.Add(t);
                        }
                    }
                }


            }
            return list;
        }
    }

}