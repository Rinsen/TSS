﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace TietoCRM.Models
{
    public class view_CustomerOffer : HashtagDocument
    {
        private int offer_number;
        public int _Offer_number { get { return offer_number; } set { offer_number = value; base._ID = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private String contact_person;
        public String Contact_person { get { return contact_person; } set { contact_person = value; } }
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

        private String module_header;
        public String Module_header { get { return module_header; } set { module_header = value; } }

        private String module_info;
        public String Module_info { get { return module_info; } set { module_info = value; } }

        private String page_foot;
        public String Page_foot { get { return page_foot; } set { page_foot = value; } }

        private String document_foot;
        public String Document_foot { get { return document_foot; } set { document_foot = value; } }

        private int summera;
        public int Summera { get { return summera; } set { summera = value; } }

        private String our_sign;
        public String Our_sign { get { return our_sign; } set { our_sign = value; } }

        private String administration;
        public String Administration { get { return administration; } set { administration = value; } }

        private String buyer;
        public String Buyer { get { return buyer; } set { buyer = value; } }

        private int orgInfoId;
        public int OrgInfoId { get { return orgInfoId; } set { orgInfoId = value; } }

        private long ssma_timestamp;
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public override int _ID
        { get { return base._ID; } set { offer_number = value; base._ID = value; } }

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
        }

        public override bool Select(string condition)
        {
            bool r = base.Select(condition);
            if (r)
            {
                this._OfferRows = view_OfferRow.getAllOfferRows(Convert.ToString(this._Offer_number), this.Area);
                this._ConsultantRows = view_ConsultantRow.getAllConsultantRow(Convert.ToString(this._Offer_number));
            }
            return r;
        }

        public view_CustomerOffer()
            : base("CustomerOffer")
        {
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_CustomerOffer p = obj as view_CustomerOffer;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this._Offer_number == p._Offer_number);
        }

        /// <summary>
        /// Gets all the offers of a specific customer
        /// </summary>
        /// <param name="customer">The customer name</param>
        /// <returns>A list of offers.</returns>
        public static List<view_CustomerOffer> getAllCustomerOffers(String customer, String our_sign = "*")
        {
            List<view_CustomerOffer> list = new List<view_CustomerOffer>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                if (customer != "*" && our_sign != "*")
                {
                    command.CommandText = "SELECT *, CAST(SSMA_TimeStamp AS BIGINT) AS ads FROM " + databasePrefix + "CustomerOffer WHERE Customer = @customer and Our_Sign = @our_sign";
                }

                if (customer == "*" && our_sign != "*")
                {
                    command.CommandText = "SELECT *, CAST(SSMA_TimeStamp AS BIGINT) AS ads FROM " + databasePrefix + "CustomerOffer WHERE Our_Sign = @our_sign";
                }

                if (customer != "*" && our_sign == "*")
                {
                    command.CommandText = "SELECT *, CAST(SSMA_TimeStamp AS BIGINT) AS ads FROM " + databasePrefix + "CustomerOffer WHERE Customer = @customer";
                }

                command.Prepare();

                if (customer != "*" && our_sign != "*")
                {
                    command.Parameters.AddWithValue("@customer", customer);
                    command.Parameters.AddWithValue("@our_sign", our_sign);
                }

                if (customer == "*" && our_sign != "*")
                {
                    command.Parameters.AddWithValue("@our_sign", our_sign);
                }

                if (customer != "*" && our_sign == "*")
                {
                    command.Parameters.AddWithValue("@customer", customer);
                }

                if(!(customer == "*" && our_sign == "*"))
                {
                    command.ExecuteNonQuery();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                view_CustomerOffer t = new view_CustomerOffer();
                                int i = 0;
                                int j = 0;
                                while (reader.FieldCount > i)
                                {
                                    String columnName = reader.GetName(i);
                                    if (columnName != "SSMA_TimeStamp" && columnName != "ID")
                                    {
                                        t.SetValue(t.GetType().GetProperties()[j].Name, reader.GetValue(i));
                                        j++;
                                    }
                                    i++;
                                }
                                t.GetHashtags();
                                t._OfferRows = view_OfferRow.getAllOfferRows(t._Offer_number.ToString(), t.Area);
                                t._ConsultantRows = view_ConsultantRow.getAllConsultantRow(t._Offer_number.ToString());
                                list.Add(t);
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static bool CustomerOfferExists(String customer)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())

            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Customer FROM " + databasePrefix + "CustomerOffer WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }


            }
        }

        public static List<view_CustomerOffer> getAllCustomerOffers()
        {
            List<view_CustomerOffer> list = new List<view_CustomerOffer>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = "SELECT *, CAST(SSMA_TimeStamp AS BIGINT) AS ads FROM " + databasePrefix + "CustomerOffer";

                command.Prepare();

                command.ExecuteNonQuery();

                List<view_OfferRow> oRows = view_OfferRow.getAllOfferRows();
                List<view_ConsultantRow> cRows = view_ConsultantRow.getAllConsultantRow();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerOffer t = new view_CustomerOffer();
                            int i = 0;
                            int j = 0;
                            while (reader.FieldCount > i)
                            {
                                String columnName = reader.GetName(i);
                                if (columnName != "SSMA_TimeStamp" && columnName != "ID")
                                {
                                    t.SetValue(t.GetType().GetProperties()[j].Name, reader.GetValue(i));
                                    j++;
                                }
                                i++;
                            }
                            t.GetHashtags();
                            t._ConsultantRows = cRows.Where(c => c.Offer_number == t._Offer_number).ToList();
                            t._OfferRows = oRows.Where(c => c.Offer_number == t._Offer_number).ToList();
                            list.Add(t);
                        }
                    }
                }
            }
            return list;
        }
        public static DataTable ExportCustomerOffersToExcel(string user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable dt = new DataTable();
                string query = "";

                if (user == "alla")
                {
                    query = "SELECT * FROM qry_CustomerOffer_Excel Order By 1";
                }
                else
                {
                    query = "SELECT * FROM qry_CustomerOffer_Excel Where Representatives like '%" + user + "%' Order By 1";
                }
                dt.TableName = user;

                SqlDataAdapter da = new SqlDataAdapter(query, connection);
                da.Fill(dt);

                return dt;
            }
        }
        public static List<String> GetCustomerOffers(string user)
        {
            List<String> list = new List<String>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String condition = "";
                if (user != null)
                    condition = "WHERE Representative like @representive";

                string query = "SELECT * FROM qry_CustomerOffer_Excel " + condition;
                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                if (user != null)
                    command.Parameters.AddWithValue("@representive", "%" + user + "%");

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        list.Add(reader.GetInt32(0).ToString());
                    }
                }
            }
            return list;
        }
    }
}