using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Appointment : SQLBaseClass
    {
        private int _id;
        public int _ID { get; set; }

        private String customer;
        public String Customer { get; set; }

        private String event_type;
        public String Event_type { get; set; }

        private String text;
        public String Text { get; set; }

        private DateTime date;
        public DateTime Date { get; set; }

        public view_Appointment() : base("Appointment")
        {
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Appointment> getAllAppointments()
        {
            List<view_Appointment> list = new List<view_Appointment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID], Customer, Date, Text, Event_type FROM " + databasePrefix + "Appointment ";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Appointment k = new view_Appointment();
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

        public static List<view_Appointment> getAllAppointments(String customer)
        {
            List<view_Appointment> list = new List<view_Appointment>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = "SELECT [ID], Customer, Event_type, Text, Date FROM " + databasePrefix + "Appointment WHERE Customer = @customer";


                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);
                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Appointment k = new view_Appointment();
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