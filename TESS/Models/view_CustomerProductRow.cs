using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;
using System.Reflection;

namespace TietoCRM.Models
{
    public class view_CustomerProductRow : SQLBaseClass
    {
        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private int article_number;
        public int Article_number { get { return article_number; } set { article_number = value; } }

        private String module;
        public String Module { get { return module; } set { module = value; } }

        private String system;
        public String System { get { return system; } set { system = value; } }

        private String classification;
        public String Classification { get { return classification; } set { classification = value; } }

        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String sign;
        public String Sign { get { return sign; } set { sign = value; } }

        private DateTime? valid_through;
        public DateTime? Valid_through { get { return valid_through; } set { valid_through = value; } }

        private String status;
        public String Status
        { get { return status; } set { status = value; } }

        [ScriptIgnore()]
        private long ssma_timestamp;
        [ScriptIgnore()]
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        private decimal sortNo;
        public decimal SortNo { get { return sortNo; } set { sortNo = value; } }

        private int discount_type;
        public int Discount_type { get { return discount_type; } set { discount_type = value; } }

        private String alias;
        public String Alias { get { return alias; } set { alias = value; } }

        public view_CustomerProductRow()
            : base("CustomerProductRow")
        {
            //ctr
        }
        /// <summary>
        /// Gets all the product rows
        /// </summary>
        /// <returns>A list of product rows.</returns>
        public static List<view_CustomerProductRow> getAllCustomerProductRows()
        {
            
            List<view_CustomerProductRow> list = new List<view_CustomerProductRow>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = @"SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias
                                FROM " + databasePrefix + "CustomerProductRow Where Discount_type = 0";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                   
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerProductRow t = new view_CustomerProductRow();
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
        /// Gets all product rows that a specific user has
        /// </summary>
        /// <param name="sign">The sign of the user.</param>
        /// <returns>A list of product rows.</returns>
        public static List<view_CustomerProductRow> getAllCustomerProductRows( String sign)
        {
         

            List<view_CustomerProductRow> list = new List<view_CustomerProductRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias FROM " + databasePrefix + "CustomerProductRow WHERE " + "Sign = @sign And Discount_type = 0";

                command.Prepare();
                command.Parameters.AddWithValue("@sign", sign);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                   
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            
                            view_CustomerProductRow t = new view_CustomerProductRow();
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
        /// Gets all product rows of a specific contract.
        /// </summary>
        /// <param name="customer">the customer that has the contract</param>
        /// <param name="contractId">The id of the contract.</param>
        /// <returns>A list of product rows.</returns>
        public static List<view_CustomerProductRow> getAllCustomerProductRows(String customer, String contractId = null)
        {
          

            List<view_CustomerProductRow> list = new List<view_CustomerProductRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
               
                // Default query
                command.CommandText = "SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, ";
                command.CommandText += "Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias FROM ";
                command.CommandText += databasePrefix + "CustomerProductRow  WHERE " + "Customer = @customer And Discount_type = 0 Order By SortNo, Classification, Article_number";
                
                // If contract id is specified
                if(contractId != null)
                {
                    command.CommandText = "SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, ";
                    command.CommandText += "Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias FROM ";
                    command.CommandText += databasePrefix + "CustomerProductRow WHERE " + "Customer = @customer AND Contract_id = @contract_id And Discount_type = 0 Order By SortNo, Classification, Article_number";
                    command.Prepare();
                    command.Parameters.AddWithValue("@contract_id", contractId);
                }
                else
                {
                    command.Prepare();
                }
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_CustomerProductRow t = new view_CustomerProductRow();
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

        public List<String> GetCsv()
        {
            List<String> csv = new List<String>();
            foreach(PropertyInfo prop in this.GetType().GetProperties())
            {
                csv.Add((prop.GetValue(this) ?? " ").ToString());
            }

            return csv;
        }
    }
}