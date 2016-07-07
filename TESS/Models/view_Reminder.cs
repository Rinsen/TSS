using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Reminder : SQLBaseClass
    {
        private int id_pk;
        public int ID_PK { get; set; }

        private DateTime start_date;
        public DateTime Start_date { get; set; }

        private int active;
        public int Active { get; set; }

        private int prio;
        public int Prio { get; set; }

        private String customer_name;
        public String Customer_name { get; set; }

        private String reminder_text;
        public String Reminder_text { get; set; }

        private DateTime _creation_date;
        public DateTime _Creation_date { get; set; }

        private String sign;
        public String Sign { get; set; }

        private String _area;
        public String _Area { get; set; }

        public view_Reminder()
            : base("Reminder")
        {
            //ctr
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Reminder> getAllReminders(String area)
        {
            List<view_Reminder> list = new List<view_Reminder>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], Start_date, Active, Prio, Customer_name, Reminder_text, Creation_date, Sign, Area FROM " + databasePrefix + "Reminder ";
                query += " Where Area = @area Order BY Start_date";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@area", area);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Reminder k = new view_Reminder();
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

        public String checkIfReminderPerCustomer(String customer, String area, String sign)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID_PK FROM " + databasePrefix + "Reminder";
                query += " Where Area = @area And Customer_name = @Customer and Active = 1 And Start_date <= Convert(char(10),GetDate(),121) Order BY Start_date";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@Customer", customer);
                command.Parameters.AddWithValue("@area", area);
                //command.Parameters.AddWithValue("@Sign", sign);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return "1";
                    }
                    else
	                {
                        return "-1";
	                }
                }
            }
        }

        public static List<view_Reminder> getRemindersPerCustomer(String customer, String area, String sign)
        {
            List<view_Reminder> list = new List<view_Reminder>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], Convert(Char(10),Start_date,121) As Start_date, Active, Prio, Customer_name, Reminder_text, Creation_date, Sign, Area FROM " + databasePrefix + "Reminder";
                query += " Where Area = @area And Customer_name = @Customer and Active = 1 And Start_date <= Convert(char(10),GetDate(),121) Order BY Start_date";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@Customer", customer);
                command.Parameters.AddWithValue("@area", area);
                //command.Parameters.AddWithValue("@Sign", sign);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Reminder k = new view_Reminder();
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

        public String checkIfReminderHighPrio(String area, String sign)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID_PK FROM " + databasePrefix + "Reminder";
                query += " Where Area = @area And Sign = @sign And (Prio = 1 Or Customer_name is Null) And Active = 1 And Start_date <= Convert(char(10),GetDate(),121) Order BY Start_date";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@area", area);
                command.Parameters.AddWithValue("@Sign", sign);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return "1";
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
        }
        public static List<view_Reminder> getRemindersHighPrio(String area, String sign)
        {
            List<view_Reminder> list = new List<view_Reminder>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], Start_date, Active, Prio, Customer_name, Reminder_text, Creation_date, Sign, Area FROM " + databasePrefix + "Reminder";
                query += " Where Area = @area And Sign = @sign And (Prio = 1 Or Customer_name is Null) and Active = 1 And Start_date <= Convert(char(10),GetDate(),121) Order BY Start_date";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@default_system", area);
                command.Parameters.AddWithValue("@Sign", sign);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Reminder k = new view_Reminder();
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
        public int deactivateReminder(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = "Update view_Reminder Set Active = 0 ";
                command.CommandText += "Where ID_PK = @id";

                command.Prepare();
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();

                return 1;
            }
        }
    }
}