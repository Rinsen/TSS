﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_CustomerOffer : SQLBaseClass
    {
        private int offer_number;
        public int _Offer_number { get { return offer_number; } set { offer_number = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String contact_person;
        public String Contact_person { get { return contact_person; } set { contact_person = value; }}
        public void SetContactPerson(String person)
        {
            foreach (view_CustomerOffer customerOffer in getAllCustomerOffers(this.Customer))
            {
                if (customerOffer.Contact_person == this.Contact_person)
                {
                    customerOffer.Contact_person = person;
                    customerOffer.Update("SSMA_timestamp = " + customerOffer.SSMA_timestamp);
                }
            }
            this.Contact_person = person;
        }

        private DateTime? offer_created;
        public DateTime? Offer_created { get { return offer_created; } set { offer_created = value; } }

        private DateTime? offer_valid;
        public DateTime? Offer_valid { get { return offer_valid; } set { offer_valid = value; } }

        private String offer_status;
        public String Offer_status { get { return offer_status; } set { offer_status = value; } }

        private String document_type;
        public String Document_type { get { return document_type; } set { document_type = value; } }

        private String document_head;
        public String Document_head { get { return document_head; } set { document_head = value; } }

        private String page_head;
        public String Page_head { get { return page_head; } set { page_head = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private String page_foot;
        public String Page_foot { get { return page_foot; } set { page_foot = value; } }

        private String document_foot;
        public String Document_foot { get { return document_foot; } set { document_foot = value; } }

        private long ssma_timestamp;
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        /// <summary>
        /// This variable does not exist in the database, and therefor there is no "_"
        /// </summary>
        private List<view_OfferRow> offerRows;
        public List<view_OfferRow> _OfferRows
        {
            get { return offerRows; }
            set { offerRows = value; }
        }

        /// <summary>
        /// This variable does not exist in the database, and therefor there is no "_"
        /// </summary>
        private List<view_ConsultantRow> consultantRow;
        public List<view_ConsultantRow> _ConsultantRows
        {
            get { return consultantRow; }
            set { consultantRow = value; }
        }

        public view_CustomerOffer(String condition)
            : base("CustomerOffer")
        {
            this.Select(condition);
            this._OfferRows = view_OfferRow.getAllOfferRows(Convert.ToString(this._Offer_number));
            this._ConsultantRows = view_ConsultantRow.getAllConsultantRow(Convert.ToString(this._Offer_number));
        }

        public view_CustomerOffer()
            : base("CustomerOffer")
        {
        }

        /// <summary>
        /// Gets all the offers of a specific customer
        /// </summary>
        /// <param name="customer">The customer name</param>
        /// <returns>A list of offers.</returns>
        public static List<view_CustomerOffer> getAllCustomerOffers(String customer)
        {
            List<view_CustomerOffer> list = new List<view_CustomerOffer>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Offer_number FROM " + databasePrefix + "CustomerOffer WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerOffer t = new view_CustomerOffer("Offer_number = " + reader["Offer_number"]);
                            list.Add(t);
                        }
                    }
                }


            }
            return list;
        }
    }

}