using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Appointment : SQLBaseClass
    {
        private int _id;
        public int _ID { get; set; }

        private DateTime date;
        public DateTime Date { get; set; }

        private String event_type;
        public String Event_type { get; set; }

        private String customer;
        public String Customer { get; set; }

        private String area;
        public String Area { get; set; }

        private String title;
        public String Title { get; set; }

        private String text;
        public String Text { get; set; }

        private String contact_person;
        public String Contact_person { get; set; }

        public view_Appointment() : base("Appointment")
        {
        }

        public String ParseToiCalEvent()
        {
            DateTime today = DateTime.Now;
            String ical =   "\r\nBEGIN:VEVENT" +
                            "\r\nUID:" + this._ID +
                            "\r\nSUMMARY:" + this.Title +
                            "\r\nDTSTAMP:" + today.ToString("yyyyMMdd") + "T" + today.ToString("hhmmss") +
                            "\r\nDTSTART:" + this.Date.ToString("yyyyMMdd") + "T" + this.Date.ToString("hhmmss") +
                            "\r\nDESCRIPTION:" + this.Text +
                            "\r\nCATEGORIES:" + this.Event_type +
                            "\r\nLOCATION:" + this.Customer +
                            "\r\nEND:VEVENT";
            return ical;
        }

        public static String ParseToiCal(List<view_Appointment> appointments)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BEGIN:VCALENDAR\r\nVERSION: 2.0\r\nNAME:TSS Appointments\r\nX-WR-CALNAME:TSS Appointments\r\n");
            sb.Append("X-PUBLISHED-TTL:PT1H\r\nPRODID:-//Tieto//NONSGML Event Calendar//EN");
            foreach(view_Appointment app in appointments)
            {
                sb.Append(app.ParseToiCalEvent());
            }
            sb.Append("\r\nEND:VCALENDAR");
            return sb.ToString();
        }

        public static String ParseToiCal(view_Appointment appointment)
        {
            List<view_Appointment> apps = new List<view_Appointment>();
            apps.Add(appointment);
            return ParseToiCal(apps);
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

                String query = "SELECT * FROM " + databasePrefix + "Appointment ";

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

                command.CommandText = "SELECT [ID], Date, Event_type, Customer, Area, Title, Text, Contact_person FROM " + databasePrefix + "Appointment WHERE Customer = @customer";


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