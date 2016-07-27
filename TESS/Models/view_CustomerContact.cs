using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_CustomerContact : SQLBaseClass
    {
        private int id;
        public int _ID { get { return id; } set { id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String contact_person;
        public String Contact_person { get { return contact_person; } set { contact_person = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private String email;
        public String Email { get { return email; } set { email = value; } }

        private String telephone;
        public String Telephone { get { return telephone; } set { telephone = value; } }

        private String mobile;
        public String Mobile { get { return mobile; } set { mobile = value; } }

        private String address;
        public String Address { get { return address; } set { address = value; } }

        private String notes;
        public String Notes { get { return notes; } set { notes = value; } }

        public view_CustomerContact()
            : base("CustomerContact")
        {
            //ctr
        }

        public String ParseTovCard()
        {
            DateTime today = DateTime.Now;
            String ical = "\r\nBEGIN:VCARD" +
                            "\r\nVERSION:" + "3.0" +
                            "\r\nUID:" + this._ID +
                            "\r\nFN:" + this.Contact_person +
                            "\r\nORG:" + this.Customer +
                            "\r\nTITLE:" + this.Title +
                            "\r\nTEL:" + this.Telephone +
                            "\r\nADR:" + this.Address +
                            "\r\nEMAIL:" + this.Email +
                            "\r\nNOTE:" + this.Notes +
                            "\r\nREV:" + today.ToString("yyyyMMdd") + "T" + today.ToString("hhmmss") + "Z" +
                            "\r\nEND:VCARD";
            return ical;
        }

        /// <summary>
        /// Gets all customer contact.
        /// </summary>
        /// <returns>A list of all contacts.</returns>
        public static List<view_CustomerContact> getAllCustomerContacts()
        {
            List<view_CustomerContact> list = new List<view_CustomerContact>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = @"SELECT * FROM " + databasePrefix + "CustomerContact";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerContact t = new view_CustomerContact();
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

        /// <summary>
        /// Gets all contacts of a specific customer.
        /// </summary>
        /// <param name="customer">The customers name/param>
        /// <returns>A list of contacts.</returns>
        public static List<view_CustomerContact> getAllCustomerContacts(String customer)
        {
            List<view_CustomerContact> list = new List<view_CustomerContact>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT * FROM " + databasePrefix + "CustomerContact WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerContact t = new view_CustomerContact();
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