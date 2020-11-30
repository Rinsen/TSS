using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_TemplateMainContract
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
        public view_TemplateMainContract(String name, MainContractType type, String value)
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
 
        public static String GetEpilog()
        {
            String epilog = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Epilog FROM dbo.Havtalsmall";

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
        public static String GetProlog()
        {
            String prolog = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Prolog FROM dbo.Havtalsmall";

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

        public static String GetTitle1()
        {
            String title = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT rubrik1 FROM dbo.Havtalsmall";

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
        public static String GetModuleText()
        {
            String text = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT ModulText FROM dbo.Havtalsmall";

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