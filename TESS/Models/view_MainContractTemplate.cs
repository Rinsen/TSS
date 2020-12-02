using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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

                String query = "SELECT [ID], [ShortDescription], [Description], [Epilog], [Prolog], [TopTitle], [ModulText] FROM " + databasePrefix + "MainContractTemplate Order By [ShortDescription]";

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

        public static view_MainContractTemplate GetMainContractTemplate(string id)
        {
            view_MainContractTemplate mainContractTemplate = new view_MainContractTemplate();
            mainContractTemplate.Select("ID = " + id);

            return mainContractTemplate;
        }
    }

}