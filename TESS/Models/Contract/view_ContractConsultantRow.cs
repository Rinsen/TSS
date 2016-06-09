using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ContractConsultantRow : SQLBaseClass
    {
        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private int code;
        public int Code { get { return code; } set { code = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private int? amount;
        public int? Amount { get { return amount; } set { amount = value; } }

        private decimal? total_price;
        public decimal? Total_price { get { return total_price; } set { total_price = value; } }

        private DateTime? created;
        public DateTime? Created { get { return created; } set { created = value; } }

        private DateTime? updated;
        public DateTime? Updated { get { return updated; } set { updated = value; } }

        public view_ContractConsultantRow()
            : base("ContractConsultantRow")
        {
            //ctr
        }

        /// <summary>
        /// Gets all consultant rows of a specific contract.
        /// </summary>
        /// <param name="contractID">The id of the contract</param>
        /// <param name="customer">the customer the contract belongs to.</param>
        /// <returns>A list on consultant rows of a specific contract.</returns>
        public static List<view_ContractConsultantRow> GetAllContractConsultantRow(String contractID, String customer)
        {
            List<view_ContractConsultantRow> list = new List<view_ContractConsultantRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [Contract_id] ,[Code] ,[Customer] ,[Amount] ,[Total_price] ,[Created] ,[Updated] FROM " + databasePrefix + "ContractConsultantRow WHERE " + "Contract_id = @contractID AND Customer = @customer";

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
                            view_ContractConsultantRow t = new view_ContractConsultantRow();
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

        /// <summary>
        /// Gets all consultant rows of a specific customer.
        /// </summary>
        /// <param name="customer">The customer to get from</param>
        /// <returns>A list with consultant rows</returns>
        public static List<view_ContractConsultantRow> GetAllContractConsultantRow(String customer)
        {
            List<view_ContractConsultantRow> list = new List<view_ContractConsultantRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [Contract_id] ,[Code] ,[Customer] ,[Amount] ,[Total_price] ,[Created] ,[Updated] FROM " + databasePrefix + "ContractConsultantRow WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractConsultantRow t = new view_ContractConsultantRow();
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