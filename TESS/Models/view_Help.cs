using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Help : SQLBaseClass
    {
        private int id;
        public int _ID { get { return id; } set { id = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private String text;
        public String Text { get { return text; } set { text = value; } }

        private String view_section;
        public String View_section { get { return view_section; } set { view_section = value; } }

        public view_Help() : base("Help")
        {

        }

        public static List<view_Help> getAllHelp(String view)
        {
            List<view_Help> list = new List<view_Help>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT * FROM " + databasePrefix + "Help WHERE " + "View_section = @view";

                command.Prepare();
                command.Parameters.AddWithValue("@view", view);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_Help t = new view_Help();
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