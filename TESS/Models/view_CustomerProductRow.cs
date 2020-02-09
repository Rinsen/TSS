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

        private String classification;
        public String Classification { get { return classification; } set { classification = value; } }

        private String module;
        public String Module { get { return module; } set { module = value; } }

        private String system;
        public String System { get { return system; } set { system = value; } }

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

        private Boolean expired;
        public Boolean Expired { get { return expired; } set { expired = value; } }

        public string Main_contract_id { get; set; }
        public DateTime? MainContract_ValidFrom { get; set; }
        public DateTime? MainContract_ValidThrough { get; set; }

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

                String query = @"SELECT Customer, Article_number, Classification, Module,System, Contract_id, Sign, Valid_through, 
                                Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias, Expired
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
                command.CommandText = @"SELECT Customer, Article_number, Classification, Module,System, Contract_id, Sign, Valid_through, 
                                        Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias, Expired
                                        FROM " + databasePrefix + "CustomerProductRow WHERE " + "Sign = @sign And Discount_type = 0";

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
        public static List<view_CustomerProductRow> getAllCustomerProductRows(String customer, String contractId = null, String area = null, bool withExpired = true)
        {
          

            List<view_CustomerProductRow> list = new List<view_CustomerProductRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                //command.CommandText = "SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, ";
                //command.CommandText += "Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias FROM ";
                //command.CommandText += databasePrefix + "CustomerProductRow WHERE " + "Customer = @customer And Discount_type = 0 Order By SortNo, Classification, Module";

                //Join mot view_Customer för att få tag på datumperiod och avtalstyp för Customer Module Report 
                if (withExpired)
                {
                    command.CommandText = @"SELECT CPR.Customer, CPR.Article_number, CPR.Classification, CPR.Module, CPR.System, CPR.Contract_id, CPR.Sign, CPR.Valid_through, 
                    CPR.Status, CAST(CPR.SSMA_timestamp AS BIGINT) AS SSMA_timestamp, CPR.SortNo, CPR.Discount_type, CPR.Alias, CPR.Expired, C.Main_contract_id, C.Valid_from as MainContract_ValidFrom, C.Valid_through as MainContract_ValidThrough
                    FROM " + databasePrefix + "CustomerProductRow CPR " +
                    "JOIN " + databasePrefix + "Contract C on C.Customer = CPR.Customer and C.Contract_id = CPR.Contract_id AND Contract_type = 'Huvudavtal' " +
                    "WHERE C.Customer = @customer And CPR.Discount_type = 0 And (C.status = 'Giltigt' or C.status = 'Sänt') ";
                }
                else
                {
                    command.CommandText = @"SELECT CPR.Customer, CPR.Article_number, CPR.Classification, CPR.Module, CPR.System, CPR.Contract_id, CPR.Sign, CPR.Valid_through, 
                    CPR.Status, CAST(CPR.SSMA_timestamp AS BIGINT) AS SSMA_timestamp, CPR.SortNo, CPR.Discount_type, CPR.Alias, CPR.Expired, C.Main_contract_id, C.Valid_from as MainContract_ValidFrom, C.Valid_through as MainContract_ValidThrough 
                    FROM " + databasePrefix + "CustomerProductRow CPR " +
                    "JOIN " + databasePrefix + "Contract C on C.Customer = CPR.Customer and C.Contract_id = CPR.Contract_id AND Contract_type = 'Huvudavtal' " +
                    "WHERE CPR.Customer = @customer And CPR.Discount_type = 0 And CPR.Expired = 0 And (C.status = 'Giltigt' or C.status = 'Sänt') ";
                }
                if (contractId != null)
                {
                    command.CommandText += "And CPR.Contract_id = @contract_id ";
                }
                if (area != null)
                {
                    command.CommandText += "And CPR.Area = @area ";
                }
                command.CommandText += "Order By C.Main_contract_id, CPR.SortNo, CPR.Classification, CPR.Module";

                // If contract id is specified
                //if (contractId != null)
                //{
                //    command.CommandText = "SELECT Customer, Article_number, Module,System, Classification, Contract_id, Sign, Valid_through, ";
                //    command.CommandText += "Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias FROM ";
                //    command.CommandText += databasePrefix + "CustomerProductRow WHERE " + "Customer = @customer AND Contract_id = @contract_id And Discount_type = 0 Order By SortNo, Classification, Module";
                //    command.Prepare();
                //    command.Parameters.AddWithValue("@contract_id", contractId);
                //}
                //else
                //{
                command.Prepare();
                //}
                command.Parameters.AddWithValue("@customer", customer);
                if (contractId != null)
                {
                    command.Parameters.AddWithValue("@contract_id", contractId);
                }
                if (area != null)
                {
                    command.Parameters.AddWithValue("@area", area);
                }

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

        public static System.Data.DataTable ExportCustomerProductsToExcel(String customer, String contractId = null, String area = null, bool withExpired = true)
        {

            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "";

                if (withExpired)
                {
                    query = @"SELECT Customer, Article_number, Classification, Module,System, Contract_id, Sign, Valid_through, 
                                        Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias, Expired 
                                        FROM " + databasePrefix + "CustomerProductRow WHERE " + "Customer = '" + customer + "' And Discount_type = 0 And Status = 'Giltigt'";
                }
                else
                {
                    query = @"SELECT Customer, Article_number, Classification, Module,System, Contract_id, Sign, Valid_through, 
                                        Status, CAST (SSMA_timestamp AS BIGINT) AS SSMA_timestamp, SortNo, Discount_type, Alias, Expired 
                                        FROM " + databasePrefix + "CustomerProductRow WHERE " + "Customer = '" + customer + "' And Discount_type = 0 And Status = 'Giltigt' And Expired = 0 ";
                }
                if (contractId != null)
                {
                    query += "And Contract_id = " + contractId;
                }
                if (area != null)
                {
                    query += "And Area = " + area;
                }
                query += "Order By SortNo, Classification, Module";
               
                dt.TableName = customer.Replace(" ","_");

                SqlDataAdapter da = new SqlDataAdapter(query, connection);
                da.Fill(dt);
            }
            return dt;
        }
    }
}