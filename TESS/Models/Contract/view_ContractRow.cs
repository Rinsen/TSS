using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
public class view_ContractRow : SQLBaseClass
	{
		private String contract_id;
		public String Contract_id { get{ return contract_id; } set{ contract_id = value; } }

		private String customer;
		public String Customer { get{ return customer; } set{ customer = value; } }

		private int article_number;
		public int Article_number { get{ return article_number; } set{ article_number = value; } }

		private int? offer_number;
		public int? Offer_number { get{ return offer_number; } set{ offer_number = value; } }

		private decimal? license;
		public decimal? License { get{ return license; } set{ license = value ?? 0; } }

		private decimal? maintenance;
        public decimal? Maintenance { get { return maintenance; } set { maintenance = value ?? 0; } }

		private DateTime? delivery_date;
		public DateTime? Delivery_date { get{ return delivery_date; } set{ delivery_date = value; } }

		private DateTime? created;
		public DateTime? Created { get{ return created; } set{ created = value; } }

		private DateTime? updated;
		public DateTime? Updated { get{ return updated; } set{ updated = value; } }

		private bool? rewritten;
		public bool? Rewritten { get{ return rewritten; } set{ rewritten = value; } }

		private bool? _new;
		public bool? New { get{ return _new; } set{ _new = value; } }

		private bool? removed;
		public bool? Removed { get{ return removed; } set{ removed = value; } }

		private DateTime? closure_date;
		public DateTime? Closure_date { get{ return closure_date; } set{ closure_date = value; } }

        private int sortnr;
        public int Sortnr { get ; set ; }

        private DateTime ssma_timestamp;
		public DateTime SSMA_timestamp { get{ return ssma_timestamp; } set{ ssma_timestamp = value; } }

		public view_ContractRow() : base("ContractRow")
		{
			//ctr
		}

        /// <summary>
        /// Gets all Contract rows of a specific contract.
        /// </summary>
        /// <param name="contractID">The id of the contract</param>
        /// <param name="customer">The customer that has the contract.</param>
        /// <returns>A list of contract rows.</returns>
        public static List<view_ContractRow> GetAllContractRows(String contractID, String customer)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], sortnr, 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp 
                                        FROM " + databasePrefix + "ContractRow WHERE " + "Contract_id = @contractID AND Customer = @customer";

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
                            view_ContractRow t = new view_ContractRow();
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
        /// Gets of contract rows of a specific customer.
        /// </summary>
        /// <param name="customer">The customer to get from</param>
        /// <returns>A list of contract rows.</returns>
        public static List<view_ContractRow> GetAllContractRows(String customer)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], sortnr, 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ContractRow WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractRow t = new view_ContractRow();
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

        public static List<view_ContractRow> GetValidContractRows(String customer)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], sortnr, 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM qry_ValidContractRow WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractRow t = new view_ContractRow();
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
        /// Gets all ContractRows for a specified DateTime period.
        /// </summary>
        /// <param name="Start">Start DateTime</param>
        /// <param name="Stop">End DateTime</param>
        /// <returns>List of ContractRows.</returns>
        public static List<view_ContractRow> GetContractRowsByDateInterval(DateTime Start, DateTime Stop)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query

                command.CommandText = @"SELECT [view_ContractRow].[Contract_id], 
                    [view_ContractRow].[Customer] ,[view_ContractRow].[Article_number] ,
                    [view_ContractRow].[Offer_number] ,[view_ContractRow].[License] ,
                    [view_ContractRow].[Maintenance] ,[view_ContractRow].[Delivery_date] ,
                    [view_ContractRow].[Created] ,[view_ContractRow].[Updated] ,
                    [view_ContractRow].[Rewritten] ,[view_ContractRow].[New] ,
                    [view_ContractRow].[Removed] ,[view_ContractRow].[Closure_date] ,
                    [view_ContractRow].[sortnr] ,
                     CAST(view_ContractRow.SSMA_TimeStamp AS BIGINT) AS SSMA_TimeStamp 
                    FROM " + databasePrefix + @"ContractRow 
                    INNER JOIN " + databasePrefix + @"Contract ON 
                    view_Contract.Customer=view_ContractRow.Customer and 
                    view_Contract.Contract_id=view_ContractRow.Contract_id WHERE
                    view_Contract.Valid_from >= @startDate AND
                    view_Contract.Valid_from <= @stopDate";
                //view_Contract.Valid_from >= Convert(datetime, '@startDate') AND
                //view_Contract.Valid_from <= Convert(datetime, '@stopDate')";


                command.Prepare();
                command.Parameters.AddWithValue("@startDate", Start);
                command.Parameters.AddWithValue("@stopDate", Stop);
                //command.Parameters.AddWithValue("@startDate", Start.ToString("yyyy-MM-dd"));
                //command.Parameters.AddWithValue("@endDate", Start.ToString("yyyy-MM-dd")));


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractRow t = new view_ContractRow();
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
        }
    }
}