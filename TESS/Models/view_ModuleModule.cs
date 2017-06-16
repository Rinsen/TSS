using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
   

    public class view_ModuleModule : SQLBaseClass
    {
        private int _id;
        public int _ID { get { return _id; } set { _id = value; } }

        private int parent_article_number;
        public int Parent_prticle_number { get { return parent_article_number; } set { parent_article_number = value; } }

        private int article_number;
        public int Article_number { get { return article_number; } set { article_number = value; } }

        public view_ModuleModule() 
            : base("ModuleModule")
        {
        }

        public static List<view_ModuleModule> getAllModuleModules(int? parent_article_number = null)
        {
            List<view_ModuleModule> list = new List<view_ModuleModule>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = " SELECT * FROM " + databasePrefix + "ModuleModule";
                SqlCommand command = new SqlCommand(query, connection);
                if (parent_article_number != null)
                {
                    query += " WHERE parent_article_number = @parent_article_number ";
                    command = new SqlCommand(query, connection);
                    command.Parameters.Add("@parent_article_number", SqlDbType.Int).Value = parent_article_number;

                }

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_ModuleModule k = new view_ModuleModule();
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
        /// Gets all child Modules related to the parent Module
        /// </summary>
        /// <param name="parent_article_number">article_number for the parent Module</param>
        /// <returns></returns>
        public static List<view_Module> getAllChildModules(int parent_article_number)
        {
            List<view_Module> moduleList = new List<view_Module>();
            List<view_ModuleModule> moduleModuleList = getAllModuleModules(parent_article_number);
            view_Module module = new view_Module();
            foreach (view_ModuleModule moduleModule in moduleModuleList)
            {
                module.Select("article_number = " + parent_article_number);
                moduleList.Add(module);
            }
            return moduleList;
        }
    }
}