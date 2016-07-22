using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TietoCRM.Models
{
    public class view_SelectOption : SQLBaseClass
    {
        private int _id;
        public int _ID { get; set; }

        private String model;
        public String Model { get; set; }

        private String property;
        public String Property { get; set; }

        private String value;
        public String Value { get; set; }

        private String text;
        public String Text { get; set; }

        public view_SelectOption() : base("SelectOption")
        {

        }

        /// <summary>
        /// Gets all information.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_SelectOption> getAllSelectOptionsWhere(String condition)
        {
            List<view_SelectOption> list = new List<view_SelectOption>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID,Model,Property,Value,Text FROM " + databasePrefix + "SelectOption WHERE " + condition;

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_SelectOption k = new view_SelectOption();
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
            sw.Stop();
            Debug.Print(sw.Elapsed.TotalSeconds.ToString());
            return list;
        }

        public static List<view_SelectOption> getAllSelectOptions()
        {
            List<view_SelectOption> list = new List<view_SelectOption>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT ID,Model,Property,Value,Text FROM " + databasePrefix + "SelectOption";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_SelectOption k = new view_SelectOption();
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            var prop = k.GetType().GetProperties()[i];
                            k.SetValue(prop.Name, reader.GetValue(i));
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