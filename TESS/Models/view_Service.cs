using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Service_test : SQLBaseClass
    {
        private int code;
        public int Code { get { return code; } set { code = value; } }

        private String description;
        public String Description { get { return description; } set { description = value; } }

        private decimal? price;
        public decimal? Price { get { return price; } set { price = value; } }

        private String offer_description;
        public String Offer_description { get { return offer_description; } set { offer_description = value; } }

        private String contract_description;
        public String Contract_description { get { return contract_description; } set { contract_description = value; } }


        public view_Service_test()
            : base("Service")
        {
            //ctr
        }

        /// <summary>
        /// Gets all services.
        /// </summary>
        /// <returns>A list of all services.</returns>
        //public static List<view_Service> getAllServices(bool withoutFormatted = false)
        //{
        //    List<view_Service> list = new List<view_Service>();

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    using (SqlCommand command = connection.CreateCommand())
        //    {
        //        connection.Open();


        //        // Default query
        //        if (withoutFormatted == false)
        //        {
        //            command.CommandText = "SELECT Code ,Description ,Price, Offer_Description, Contract_Description FROM " + databasePrefix + "Service";
        //        }
        //        else
        //        {
        //            command.CommandText = @"SELECT Code ,Description ,Price, 
        //                                    Case When isnull(offer_description,'') = '' Then '' Else 'Ifyllt' End As Offer_descritption, 
        //                                    Case When isnull(contract_description,'') = '' Then '' Else 'Ifyllt' End As Contract_descritption 
        //                                    FROM " + databasePrefix + "Service";
        //        }

        //        command.Prepare();
        //        command.ExecuteNonQuery();

        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            int count = 1;
        //            while (reader.Read())
        //            {
        //                if (reader.HasRows)
        //                {
        //                    view_Service t = new view_Service();
        //                    int i = 0;
        //                    while (reader.FieldCount > i)
        //                    {
        //                        t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
        //                        i++;
        //                    }
        //                    list.Add(t);
        //                    count++;
        //                }
        //            }
        //        }
        //    }
        //    return list;
        //}
    }

}