using System;
using System.Collections.Generic;
using System.Data;
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
        public int Article_number { get { return article_number; } set { article_number= value; } }

        private int feature_id;
        public int Feature_Id { get { return feature_id; } set { feature_id = value; } }

        public view_ModuleFeature()
            : base("ModuleFeature")
        {
            //ctr
        }

        public static List<view_ModuleFeature> getAllModuleFeatures(int? article_number = null)
        {
            List<view_ModuleFeature> list = new List<view_ModuleFeature>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = " SELECT * FROM " + databasePrefix + "ModuleFeature";
                SqlCommand command = new SqlCommand(query, connection);
                if (article_number != null)
                {
                    query += " WHERE [Article_number] = @article_number ";
                    command = new SqlCommand(query, connection);
                    command.Parameters.Add("@article_number", SqlDbType.Int).Value = article_number;
                    
                }

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

        public static List<FeatureService.Features> getAllFeatures(int article_number)
        {
            List<view_ModuleFeature> moduleFeaturesList= getAllModuleFeatures(article_number);
            List<FeatureService.Features> featureList= new List<FeatureService.Features>();
            foreach (view_ModuleFeature moduleFeature in moduleFeaturesList)
            {
                var client = FeatureServiceProxy.GetFeaturesClient();
                if(client != null)
                    featureList.Add(client.GetFeature(moduleFeature.Feature_Id));
            }
            return featureList;
        }

        /// <summary>
        /// Creates a list with the names of all relations, in ascending order, for provided Feature
        /// </summary>
        /// <param name="feature">Feature to look for parent in</param>
        /// <param name="relation">list to build the relations recursively</param>
        /// <returns></returns>
        public static List<String> GetRelationByParent(FeatureService.Features feature, List<String> relation = null)
        {
            if (relation == null)
            {
                relation = new List<string>();
            }
            if (feature.Parent == null) // base case: continue until Feature has no Parent
            {
                return relation;
            }
            else // recursive case: add Parent Name to list and continue until base case is fullfilled
            {
                relation.Insert(0, feature.Parent.Text);
                return GetRelationByParent(feature.Parent, relation);
            }
        }
    }
}