using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class MainContractText
    {
        public enum MainContractType
        {
            MainHead,
            Subheading,
            Text
        };

        private MainContractType type;
        public MainContractType Type
        {
            get { return type; }
            set { type = value; }
        }

        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private String value;
        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private List<MainContractText> children;
        public List<MainContractText> Children
        {
            get { return children; }
            set { children = value; }
        }

        /// <summary>
        /// init the object with certain values.
        /// </summary>
        /// <param name="name">The column name in the database</param>
        /// <param name="type">The heading type of the object, MainHead, Subheading, Text</param>
        /// <param name="value">The value the database holds</param>
        public MainContractText(String name, MainContractType type, String value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
            this.Children = new List<MainContractText>();
        }
        /// <summary>
        /// Updates the given column name with given value to the database
        /// </summary>
        /// <param name="name">The column name in the database</param>
        /// <param name="value">The value you want for that given column name in the database</param>
        public static void Update(String name, String value)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = new SqlCommand(String.Format("UPDATE dbo.Havtalsmall SET {0}=@value", name), connection))
            {
                connection.Open();

                /*SqlParameter nameParam = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 100);
                nameParam.Value = name;*/
                SqlParameter valueParam = new SqlParameter("@value", System.Data.SqlDbType.NVarChar, -1);
                valueParam.Value = value;
               
                
                //command.Parameters.Add(nameParam);
                command.Parameters.Add(valueParam);
                command.Prepare();
                int a = command.ExecuteNonQuery();


              


            }
        }

        /// <summary>
        /// Get all the main contract templates from the database and parse it in to a list of objects of this class
        /// </summary>
        /// <returns>Returns a list of MainContractText</returns>
        public static List<MainContractText> getAllMainContractTexts()
        {

            List<MainContractText> list = new List<MainContractText>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT * FROM dbo.Havtalsmall";

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            MainContractText rubrik1 = new MainContractText("rubrik1", MainContractType.MainHead, reader.GetString(0));
                            rubrik1.Children.Add(new MainContractText("text1", MainContractType.Text, reader.GetString(28)));
                            rubrik1.Children.Add(new MainContractText("text2", MainContractType.Text, reader.GetString(29)));
                            list.Add(rubrik1);

                            MainContractText rubrik2 = new MainContractText("rubrik2", MainContractType.MainHead, reader.GetString(1));
                            rubrik2.Children.Add(new MainContractText("text3", MainContractType.Text, reader.GetString(30)));
                            rubrik2.Children.Add(new MainContractText("text4", MainContractType.Text, reader.GetString(31)));
                            MainContractText rubrik5 = new MainContractText("rubrik5", MainContractType.Subheading, reader.GetString(4));
                            rubrik5.Children.Add(new MainContractText("text5", MainContractType.Text, reader.GetString(32)));
                            rubrik2.Children.Add(rubrik5);
                            MainContractText rubrik6 = new MainContractText("rubrik6", MainContractType.Subheading, reader.GetString(5));
                            rubrik6.Children.Add(new MainContractText("text6", MainContractType.Text, reader.GetString(33)));
                            rubrik2.Children.Add(rubrik6);
                            MainContractText rubrik7 = new MainContractText("rubrik7", MainContractType.Subheading, reader.GetString(6));
                            rubrik7.Children.Add(new MainContractText("text7", MainContractType.Text, reader.GetString(34)));
                            rubrik2.Children.Add(rubrik7);
                            MainContractText rubrik8 = new MainContractText("rubrik8", MainContractType.Subheading, reader.GetString(7));
                            rubrik8.Children.Add(new MainContractText("text8", MainContractType.Text, reader.GetString(35)));
                            rubrik2.Children.Add(rubrik8);
                            list.Add(rubrik2);

                            MainContractText rubrik9 = new MainContractText("rubrik9", MainContractType.MainHead, reader.GetString(8));
                            MainContractText rubrik10 = new MainContractText("rubrik10", MainContractType.Subheading, reader.GetString(9));
                            rubrik10.Children.Add(new MainContractText("text9", MainContractType.Text, reader.GetString(36)));
                            rubrik9.Children.Add(rubrik10);
                            MainContractText rubrik11 = new MainContractText("rubrik11", MainContractType.Subheading, reader.GetString(10));
                            rubrik11.Children.Add(new MainContractText("text10", MainContractType.Text, reader.GetString(37)));
                            rubrik9.Children.Add(rubrik11);
                            MainContractText rubrik12 = new MainContractText("rubrik12", MainContractType.Subheading, reader.GetString(11));
                            rubrik12.Children.Add(new MainContractText("text11", MainContractType.Text, reader.GetString(38)));
                            rubrik9.Children.Add(rubrik12);
                            list.Add(rubrik9);

                            MainContractText rubrik13 = new MainContractText("rubrik13", MainContractType.MainHead, reader.GetString(12));
                            MainContractText rubrik14 = new MainContractText("rubrik14", MainContractType.Subheading, reader.GetString(13));
                            rubrik14.Children.Add(new MainContractText("text12", MainContractType.Text, reader.GetString(39)));
                            rubrik13.Children.Add(rubrik14);
                            MainContractText rubrik15 = new MainContractText("rubrik15", MainContractType.Subheading, reader.GetString(14));
                            rubrik15.Children.Add(new MainContractText("text13", MainContractType.Text, reader.GetString(40)));
                            rubrik13.Children.Add(rubrik15);
                            MainContractText rubrik16 = new MainContractText("rubrik16", MainContractType.Subheading, reader.GetString(15));
                            rubrik16.Children.Add(new MainContractText("text14", MainContractType.Text, reader.GetString(41)));
                            rubrik13.Children.Add(rubrik16);
                            MainContractText rubrik17 = new MainContractText("rubrik17", MainContractType.Subheading, reader.GetString(16));
                            rubrik17.Children.Add(new MainContractText("text15", MainContractType.Text, reader.GetString(42)));
                            rubrik13.Children.Add(rubrik17);
                            MainContractText rubrik18 = new MainContractText("rubrik18", MainContractType.Subheading, reader.GetString(17));
                            rubrik18.Children.Add(new MainContractText("text16", MainContractType.Text, reader.GetString(43)));
                            rubrik13.Children.Add(rubrik18);
                            list.Add(rubrik13);

                            MainContractText rubrik19 = new MainContractText("rubrik19", MainContractType.MainHead, reader.GetString(18));
                            MainContractText rubrik20 = new MainContractText("rubrik20", MainContractType.Subheading, reader.GetString(19));
                            rubrik20.Children.Add(new MainContractText("text17", MainContractType.Text, reader.GetString(44)));
                            rubrik19.Children.Add(rubrik20);
                            MainContractText rubrik21 = new MainContractText("rubrik21", MainContractType.Subheading, reader.GetString(20));
                            rubrik21.Children.Add(new MainContractText("text18", MainContractType.Text, reader.GetString(45)));
                            rubrik19.Children.Add(rubrik21);
                            list.Add(rubrik19);

                            MainContractText rubrik22 = new MainContractText("rubrik22", MainContractType.MainHead, reader.GetString(21));
                            MainContractText rubrik23 = new MainContractText("rubrik23", MainContractType.Subheading, reader.GetString(22));
                            rubrik23.Children.Add(new MainContractText("text19", MainContractType.Text, reader.GetString(46)));
                            rubrik22.Children.Add(rubrik23);
                            MainContractText rubrik24 = new MainContractText("rubrik24", MainContractType.Subheading, reader.GetString(23));
                            rubrik24.Children.Add(new MainContractText("text20", MainContractType.Text, reader.GetString(47)));
                            rubrik22.Children.Add(rubrik24);
                            list.Add(rubrik22);

                            MainContractText rubrik25 = new MainContractText("rubrik25", MainContractType.MainHead, reader.GetString(24));
                            rubrik25.Children.Add(new MainContractText("text21", MainContractType.Text, reader.GetString(48)));
                            rubrik25.Children.Add(new MainContractText("rubrik26", MainContractType.Subheading, reader.GetString(25)));
                            rubrik25.Children.Add(new MainContractText("rubrik27", MainContractType.Subheading, reader.GetString(26)));
                            rubrik25.Children.Add(new MainContractText("text22", MainContractType.Text, reader.GetString(49)));
                            list.Add(rubrik25);

                            MainContractText rubrik28 = new MainContractText("rubrik28", MainContractType.MainHead, reader.GetString(27));
                            rubrik28.Children.Add(new MainContractText("text23", MainContractType.Text, reader.GetString(50)));
                            list.Add(rubrik28); 
                        }
                    }
                }


            }
            return list;
        }

        public static String GetEpilog(string id)
        {
            String epilog = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Epilog FROM dbo.Havtalsmall where ID = " + id;

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            epilog = reader["Epilog"].ToString();
                        }
                    }
                }
            }
            return epilog;
        }
        public static String GetProlog(string id)
        {
            String prolog = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Prolog FROM dbo.Havtalsmall where ID = " + id;

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            prolog = reader["Prolog"].ToString();
                        }
                    }
                }
            }
            return prolog;
        }

        public static String GetTitle1(string id)
        {
            String title = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT rubrik1 FROM dbo.Havtalsmall where ID = " + id;

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            title = reader["rubrik1"].ToString();
                        }
                    }
                }
            }
            return title;
        }
        public static String GetModuleText(string id)
        {
            String text = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT ModulText FROM dbo.Havtalsmall where ID = " + id;

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            text = reader["ModulText"].ToString();
                        }
                    }
                }
            }
            return text;
        }
    }
}