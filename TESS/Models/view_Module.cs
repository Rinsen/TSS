using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Module : SQLBaseClass
    {
        private float article_number;
        public float Article_number { get { return article_number; } set { article_number = value; } }

        private String module;
        public String Module { get { return module; } set { module = value; } }

        private String description;
        public String Description { get { return description; } set { description = value; } }

        private decimal? price_category;
        public decimal? Price_category { get { return price_category; } set { price_category = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private String system;
        public String System { get { return system; } set { system = value; } }

        private String classification;
        public String Classification { get { return classification; } set { classification = value; } }

        private bool? fixed_price;
        public bool? Fixed_price { get { return fixed_price; } set { fixed_price = value; } }

        private bool? expired;
        public bool? Expired { get { return expired; } set { expired = value; } }

        private String comment;
        public String Comment { get { return comment; } set { comment = value; } }

        private int discount;
        public int Discount { get { return discount; } set { discount = value; } }

        private int discount_type;
        public int Discount_type { get { return discount_type; } set { discount_type = value; } }

        private int multiple_type;
        public int Multiple_type { get { return multiple_type; } set { multiple_type = value; } }

        private long ssma_timestamp;
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_Module()
            : base("Module")
        {
            //ctr
        }

        /// <summary>
        /// Gets all modules
        /// </summary>
        /// <returns>A list of modules</returns>
        public static List<view_Module> getAllModules()
        {
            List<view_Module> list = new List<view_Module>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [Article_number] ,[Module] ,[Description] ,[Price_category] ,[Area] ,";
                query += "[System] ,[Classification] ,[Fixed_price] ,[Expired] ,[Comment], Discount, Discount_type, Multiple_type ,CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "Module";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Module t = new view_Module();
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
            return list;
        }

        /// <summary>
        /// Gets all the modules with the price from the tariff.
        /// </summary>
        /// <param name="system">The system of the module</param>
        /// <param name="customer">The customer</param>
        /// <param name="classification">The classification of the module</param>
        /// <returns>A list of dictionaries with the collumn name as key and the value of the collumn as value.</returns>
        public static List<Dictionary<String, object>> getModuleWithCorrectPrice(String system, String customer, String classification)
        { 
            String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;

            List<Dictionary<String, object>> resultList = new List<Dictionary<String, object>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                String queryText = @"SELECT view_Module.Article_number, view_Module.Module, view_Tariff.License, view_Tariff.Maintenance 
                                    FROM view_Module                                                                                       
                                    JOIN view_Tariff                                                                                       
                                    on view_Module.Price_category = view_Tariff.Price_category
                                    WHERE System = @System AND Classification = @classification 
                                    AND Inhabitant_level = (
                                        Select ISNULL(Inhabitant_level, 1) AS I_level from view_Customer
                                        where Customer = @customer
                                    )
                                    order by Article_number asc";

                // Default query
                command.CommandText = queryText;

                command.Prepare();
                command.Parameters.AddWithValue("@System", system);
                command.Parameters.AddWithValue("@classification", classification);
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, object> result = new Dictionary<String, object>();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {

                                result.Add(reader.GetName(i), reader.GetValue(i));



                                i++;
                            }
                            result["Article_number"] = result["Article_number"].ToString();
                            
                            resultList.Add(result);
                        }
                    }
                }
            }
            return resultList;
        }
    }

}