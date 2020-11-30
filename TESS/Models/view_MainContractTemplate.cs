using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace TietoCRM.Models
{
    public class view_MainContractTemplate : SQLBaseClass
    {
        private decimal id;
        public decimal ID { get { return id; } set { id = value; } }

        private string shortDescription;
        public string ShortDescription { get { return shortDescription; } set { shortDescription = value; } }

        private string description;
        public string Description { get { return description; } set { description = value; } }

        private string epilog;
        public string Epilog { get { return epilog; } set { epilog = value; } }

        private string prolog;
        public string Prolog { get { return prolog; } set { prolog = value; } }

        private string topTitle;
        public string TopTitle { get { return topTitle; } set { topTitle = value; } }

        private string modulText;
        public string ModulText { get { return modulText; } set { modulText = value; } }

        public view_MainContractTemplate()
            : base("MainContractTemplate")
        {
            //ctr
        }


        /// <summary>
        /// Gets all main contract templates.
        /// </summary>
        /// <returns>A lsit of main contract templates.</returns>
        public static List<view_MainContractTemplate> getAllMainContractTemplates()
        {
            List<view_MainContractTemplate> list = new List<view_MainContractTemplate>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID], [ShortDescription], [Description], [Epilog], [Prolog], [ModulText] FROM " + databasePrefix + "MainContractTemplate Order By [ShortDescription]";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_MainContractTemplate k = new view_MainContractTemplate();
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
        /// <summary>
        /// Updates the given column name with given value to the database
        /// </summary>
        /// <param name="name">The column name in the database</param>
        /// <param name="value">The value you want for that given column name in the database</param>
        public static void Update(String name, String value)
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            using (SqlCommand command = new SqlCommand(String.Format("UPDATE " + databasePrefix + "MainContractTemplate SET {0}=@value", name), connection))
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

    }

}