using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ModuleFeature : SQLBaseClass
    {
        private int _id;
        public int _ID { get { return _id; } set { _id = value; } }

        private int article_number;
        public int Article_Number { get { return article_number; } set { article_number= value; } }

        private int feature_id;
        public int Feature_Id { get { return feature_id; } set { feature_id = value; } }

        public view_ModuleFeature()
            : base("ModuleFeature")
        {
            //ctr
        }

        public static List<view_ModuleFeature> getAllModuleFeatures()
        {
            List<view_ModuleFeature> list = new List<view_ModuleFeature>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = " SELECT * FROM " + databasePrefix + "ModuleFeature";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_ModuleFeature k = new view_ModuleFeature();
                        int i = 0;
                        while(reader.FieldCount > i)
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