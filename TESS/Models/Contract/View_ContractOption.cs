using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ContractOption : SQLBaseClass
    {
        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private int article_number;
        public int Article_number { get { return article_number; } set { article_number = value; } }

        private int? offer_number;
        public int? Offer_number { get { return offer_number; } set { offer_number = value; } }

        private decimal? license;
        public decimal? License { get { return license; } set { license = value; } }

        private decimal? maintenance;
        public decimal? Maintenance { get { return maintenance; } set { maintenance = value; } }

        private DateTime? date;
        public DateTime? Date { get { return date; } set { date = value; } }

        private bool? choice;
        public bool? Choice { get { return choice; } set { choice = value; } }

        private String supplementary_contract;
        public String Supplementary_contract { get { return supplementary_contract; } set { supplementary_contract = value; } }

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_ContractOption()
            : base("ContractOption")
        {
            //ctr
        }

        /// <summary>
        /// Gets all options in a specific contract.
        /// </summary>
        /// <param name="contractID">The id of the contract/param>
        /// <param name="customer">The customer that has the contract.</param>
        /// <returns>A list of contracts</returns>
        public static List<view_ContractOption> getAllOptions(String contractID, String customer)
        {
            List<view_ContractOption> list = new List<view_ContractOption>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [Contract_id] ,[Customer] ,[Article_number] ,[Offer_number] ,[License] ,[Maintenance] ,[Date] ,[Choice] ,[Supplementary_contract] , CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ContractOption WHERE " + "Contract_id = @contractID AND Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@contractID", contractID);
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractOption t = new view_ContractOption();
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