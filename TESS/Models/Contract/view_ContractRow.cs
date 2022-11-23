using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TietoCRM.Extensions;
using System.Dynamic;
using System.Linq;

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

        private int fixed_price;
        public int Fixed_price { get ; set ; }

        private DateTime ssma_timestamp;
		public DateTime SSMA_timestamp { get{ return ssma_timestamp; } set{ ssma_timestamp = value; } }

        private String alias;
        public String Alias { get { return alias; } set { alias = value; } }

        private bool _includeDependencies;
        public bool IncludeDependencies { get { return _includeDependencies; } set { _includeDependencies = value; } }

        private string removedFromContractId;
        public string RemovedFromContractId { get { return removedFromContractId; } set { removedFromContractId = value; } }

        private static int ASort { get; set; }
        //private int ASort;
        private static string OrderBy { get; set; }

        public view_ContractRow() : base("ContractRow")
		{
            //ctr
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_ContractRow p = obj as view_ContractRow;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Contract_id == p.Contract_id) && (this.Customer == p.Customer);
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
                command.CommandText = @"SELECT CR.Contract_id, CR.Customer ,CR.Article_number, CR.Offer_number, CR.License, CR.Maintenance,
                                        CR.Delivery_date, CR.Created, CR.Updated, CR.Rewritten, CR.New, CR.Removed, CR.Closure_date, CR.Fixed_price, 
                                        CAST(CR.SSMA_timestamp AS BIGINT) AS SSMA_timestamp, CR.Alias, CR.IncludeDependencies, CR.RemovedFromContractId
                                        FROM " + databasePrefix + "ContractRow CR " +
                                        "JOIN " + databasePrefix + "Module M on M.Article_number = CR.Article_number " +
                                        "WHERE " + "(CR.Contract_id = @contractID AND CR.Customer = @customer) OR CR.RemovedFromContractId = @contractID Order By " + GetOrderByForGetAllContractRows();

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
        /// Gets all Contract rows.
        /// </summary>
        /// <returns>A list of contract rows.</returns>
        public static List<view_ContractRow> GetAllContractRows()
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], [Fixed_price], 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp, [Alias], [IncludeDependencies] 
                                        FROM " + databasePrefix + "ContractRow Order By " + GetOrderBy();

                command.Prepare();


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

                //Ersätt med denna för att få in status på kontraktet
                  //SELECT CR.Contract_id, CR.Customer, CR.Article_number, CR.Offer_number, CR.License, CR.Maintenance,
                  //CR.Delivery_date, CR.Created, CR.Updated, CR.Rewritten, CR.New, CR.Removed, CR.Closure_date, CR.Fixed_price, 
                  //CAST(CR.SSMA_timestamp AS BIGINT) AS SSMA_timestamp, CR.Alias, C.[status] AS Contract_Status FROM view_ContractRow CR
                  //JOIN view_Contract C on C.Customer = CR.Customer and C.Contract_id = CR.Contract_id
                  //WHERE CR.Customer = 'Ale kommun'

                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], [Fixed_price], 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp, [Alias], [IncludeDependencies] FROM " + databasePrefix + "ContractRow WHERE " + "Customer = @customer Order By " + GetOrderBy();

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
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], [Fixed_price], 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp, [Alias] FROM qry_ValidContractRow WHERE " + "Customer = @customer Order By " + GetOrderBy();

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

        public static List<view_ContractRow> GetValidContractRows(int articleNumber)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], [Fixed_price], 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp, [Alias] FROM qry_ValidContractRow WHERE Article_number=@articleNumber Order By " + GetOrderBy();

                command.Prepare();
                command.Parameters.AddWithValue("@articleNumber", articleNumber);
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
                    [view_ContractRow].[Fixed_price] ,
                     CAST(view_ContractRow.SSMA_TimeStamp AS BIGINT) AS SSMA_TimeStamp ,
                    [view_ContractRow].[Alias] 
                    FROM " + databasePrefix + @"ContractRow 
                    INNER JOIN " + databasePrefix + @"Contract ON 
                    view_Contract.Customer=view_ContractRow.Customer and 
                    view_Contract.Contract_id=view_ContractRow.Contract_id WHERE
                    view_Contract.Valid_from >= @startDate AND
                    view_Contract.Valid_from <= @stopDate AND
                    view_ContractRow.Rewritten = 0 AND view_ContractRow.Removed = 0 Order By " + GetOrderBy();
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

        /// <summary>
        /// Gets all ContractRows for a specified DateTime period.
        /// </summary>
        /// <param name="Start">Start DateTime</param>
        /// <param name="Stop">End DateTime</param>
        /// <param name="customers">Customers</param>
        /// <param name="articleNumbers">Articles</param>
        /// <returns>List of ContractRows.</returns>
        public static List<view_ContractRow> GetContractRowsByDateIntervalCustomersAndArticleNumbers(DateTime Start, DateTime Stop, List<string> customers, List<int> articleNumbers)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                var customerString = "";
                var articleNumberString = "";

                if (customers != null && customers.Count > 0)
                {
                    customerString = string.Join(",", customers.Select(s => "'" + s + "'").ToArray());
                }
                if(articleNumbers != null && articleNumbers.Count > 0)
                {
                    articleNumberString = string.Join(",", articleNumbers.Select(n => n.ToString()).ToArray());
                }

                // Default query
                command.CommandText = @"SELECT [view_ContractRow].[Contract_id], 
                    [view_ContractRow].[Customer] ,[view_ContractRow].[Article_number] ,
                    [view_ContractRow].[Offer_number] ,[view_ContractRow].[License] ,
                    [view_ContractRow].[Maintenance] ,[view_ContractRow].[Delivery_date] ,
                    [view_ContractRow].[Created] ,[view_ContractRow].[Updated] ,
                    [view_ContractRow].[Rewritten] ,[view_ContractRow].[New] ,
                    [view_ContractRow].[Removed] ,[view_ContractRow].[Closure_date] ,
                    [view_ContractRow].[Fixed_price] ,
                     CAST(view_ContractRow.SSMA_TimeStamp AS BIGINT) AS SSMA_TimeStamp ,
                    [view_ContractRow].[Alias] 
                    FROM " + databasePrefix + @"ContractRow 
                    INNER JOIN " + databasePrefix + @"Contract ON 
                    view_Contract.Customer=view_ContractRow.Customer and 
                    view_Contract.Contract_id=view_ContractRow.Contract_id WHERE
                    view_Contract.Valid_from >= @startDate AND
                    view_Contract.Valid_from <= @stopDate";
                
                if(!string.IsNullOrEmpty(customerString))
                {
                    command.CommandText += " AND view_ContractRow.Customer IN (" + customerString + ")";
                }
                
                if(!string.IsNullOrEmpty(articleNumberString))
                {
                    command.CommandText += " AND view_ContractRow.Article_number IN(" + articleNumberString + ")";
                }
                
                command.CommandText += " ORDER BY (SELECT COUNT(*) FROM [view_ContractRow] countTest INNER JOIN view_Contract C ON" +
                    " C.Customer = countTest.Customer AND C.Contract_id = countTest.Contract_id WHERE C.Valid_from >= @startDate AND" +
                    " C.Valid_from <= @stopDate AND view_ContractRow.Article_number = countTest.Article_number) DESC";

                command.Prepare();
                command.Parameters.AddWithValue("@startDate", Start);
                command.Parameters.AddWithValue("@stopDate", Stop);

                try
                {
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
                catch (Exception ex)
                {
                    throw ex;
                }

                return list;
            }
        }

        /// <summary>
        /// Hämtar kontraktrader för att bygga ihop modultexter på kontraktet
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="contract_id"></param>
        /// <returns></returns>
        public List<dynamic> GetContractRowsForModuleInfo(string customer, string contract_id)
        {
            var list = new List<dynamic>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                // Default query
                //command.CommandText = @"SELECT C.Article_number, C.Alias, M.Contract_Description FROM " + databasePrefix + @"ContractRow C 
                //                        Inner Join  " + databasePrefix + @"Module M On M.Article_number = C.Article_number 
                //                        Where IsNull(M.Contract_Description,'') <> '' And C.Customer = @customer And C.Contract_id = @contract_id Order By " + GetOrderBy();

                command.CommandText = @"SELECT Q.Alias, Q.Description, Q.Typ, Q.Art_id, M.System AS System FROM qry_ContractArtDescription Q 
                                        JOIN " + databasePrefix + @"Module M ON M.Article_number = Q.Art_id
                                        JOIN " + databasePrefix + @"Sector S ON S.System = M.System and S.Classification = M.Classification
                                        WHERE Q.Avtalsid = @contract_id AND Q.Kund = @customer 
                                        ORDER BY " + GetOrderByForQry();

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);
                command.Parameters.AddWithValue("@contract_id", contract_id);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            dynamic t = new ExpandoObject();
                            t.Alias = reader.GetValue(0);
                            t.Contract_description = reader.GetValue(1);
                            list.Add(t);
                        }
                    }
                }


            }
            return list;

        }

        private static string GetOrderBy()
        {
            ASort = System.Web.HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "Alias";
            if (ASort == 2 || ASort == 4) return "Classification, Alias";
            if (ASort == 3) return "Classification, Article_number";
            return "Alias";
        }

        private static string GetOrderByForGetAllContractRows()
        {
            ASort = System.Web.HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "CR.Alias";
            if (ASort == 2 || ASort == 4) return "M.Classification, CR.Alias";
            if (ASort == 3) return "M.Classification, CR.Article_number";
            return "CR.Alias";
        }

        private static string GetOrderByForQry()
        {
            ASort = System.Web.HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "Typ, System, Alias";
            if (ASort == 2) return "Typ, M.Classification, Alias";
            if (ASort == 3) return "Typ, M.Classification, Art_id";
            if (ASort == 4) return "Typ, S.SortNo, M.Classification, ISNULL(NULLIF(Sort_order, 0), 99), Alias";
            return "Typ, System, Alias";
        }

        public static System.Data.DataTable ExportValidContractRowsToExcel(string articleNumber, bool allModules)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();


                // Default query
               string query  = @"SELECT Article_number, alias as Module, Customer, Contract_id , Classif as Classification, our_sign as Representative FROM qry_ValidContractRow WHERE Article_number in (" + articleNumber + ") Order By Customer, Contract_id";

                if(allModules)
                {
                    //Maximum chars in worksheet name is 31 chars..
                    dt.TableName = "ModuleReport_All_Modules";
                }
                else
                {
                    dt.TableName = "ModuleReport_Selection";
                }

                SqlDataAdapter da = new SqlDataAdapter(query, connection);
                da.Fill(dt);
            }
            return dt;
        }
        public static System.Data.DataTable ExportContractRowsByDateIntervalToExcel(DateTime Start, DateTime Stop)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Default query

                string query = @"SELECT [view_ContractRow].[Contract_id], 
                    [view_ContractRow].[Customer] ,[view_ContractRow].[Article_number] ,
                    [view_ContractRow].[Offer_number] ,[view_ContractRow].[License] ,
                    [view_ContractRow].[Maintenance] ,[view_ContractRow].[Delivery_date] ,
                    [view_ContractRow].[Created] ,[view_ContractRow].[Updated] ,
                    [view_ContractRow].[Rewritten] ,[view_ContractRow].[New] ,
                    [view_ContractRow].[Removed] ,[view_ContractRow].[Closure_date] ,
                    [view_ContractRow].[Fixed_price] ,
                     CAST(view_ContractRow.SSMA_TimeStamp AS BIGINT) AS SSMA_TimeStamp ,
                    [view_ContractRow].[Alias] 
                    FROM " + databasePrefix + @"ContractRow 
                    INNER JOIN " + databasePrefix + @"Contract ON 
                    view_Contract.Customer=view_ContractRow.Customer and 
                    view_Contract.Contract_id=view_ContractRow.Contract_id WHERE
                    view_Contract.Valid_from >= '" + Start.ToShortDateString() + @"' AND
                    view_Contract.Valid_from <= '" + Stop.ToShortDateString() + @"' AND
                    view_ContractRow.Rewritten = 0 AND view_ContractRow.Removed = 0Order By " + GetOrderBy();
                //view_Contract.Valid_from >= Convert(datetime, '@startDate') AND
                //view_Contract.Valid_from <= Convert(datetime, '@stopDate')";


                //command.Prepare();
                //command.Parameters.AddWithValue("@startDate", Start);
                //command.Parameters.AddWithValue("@stopDate", Stop);
                ////command.Parameters.AddWithValue("@startDate", Start.ToString("yyyy-MM-dd"));
                ////command.Parameters.AddWithValue("@endDate", Start.ToString("yyyy-MM-dd")));

                dt.TableName = "ContractSoldReport";

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, connection);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            return dt;
            }

        public static System.Data.DataTable ExportContractRowsByCustomerArticleAndDateIntervalToExcel(DateTime Start, DateTime Stop, List<string> customers, List<int> articleNumbers)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var customerString = "";
                var articleNumberString = "";

                if (customers != null && customers.Count > 0)
                {
                    customerString = string.Join(",", customers.Select(s => "'" + s + "'").ToArray());
                }
                if (articleNumbers != null && articleNumbers.Count > 0)
                {
                    articleNumberString = string.Join(",", articleNumbers.Select(n => n.ToString()).ToArray());
                }

                // Default query
                string query = @"SELECT [view_ContractRow].[Contract_id], 
                    [view_ContractRow].[Customer] ,[view_ContractRow].[Article_number] ,
                    [view_ContractRow].[Offer_number] ,[view_ContractRow].[License] ,
                    [view_ContractRow].[Maintenance] ,[view_ContractRow].[Delivery_date] ,
                    [view_ContractRow].[Rewritten] ,[view_ContractRow].[New] ,
                    [view_ContractRow].[Removed] ,[view_ContractRow].[Closure_date] ,
                    [view_ContractRow].[Alias] 
                    FROM " + databasePrefix + @"ContractRow 
                    INNER JOIN " + databasePrefix + @"Contract ON 
                    view_Contract.Customer=view_ContractRow.Customer and 
                    view_Contract.Contract_id=view_ContractRow.Contract_id WHERE
                    view_Contract.Valid_from >= '" + Start.ToShortDateString() + @"' AND
                    view_Contract.Valid_from <= '" + Stop.ToShortDateString() + @"'";

                if (!string.IsNullOrEmpty(customerString))
                {
                    query += " AND view_ContractRow.Customer IN (" + customerString + ")";
                }

                if (!string.IsNullOrEmpty(articleNumberString))
                {
                    query += " AND view_ContractRow.Article_number IN(" + articleNumberString + ")";
                }

                query += " ORDER BY (SELECT COUNT(*) FROM [view_ContractRow] countTest INNER JOIN view_Contract C ON" +
                    " C.Customer = countTest.Customer AND C.Contract_id = countTest.Contract_id WHERE C.Valid_from >= '" + Start.ToShortDateString() + @"' AND" +
                    " C.Valid_from <= '" + Stop.ToShortDateString() + @"' AND view_ContractRow.Article_number = countTest.Article_number) DESC";
                
                dt.TableName = "CustomerProductGrowthReport";

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(query, connection);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return dt;
        }

        /// <summary>
        /// Hämtar kundens omskrivna OCH avslutade moduler (via avtalstypen modulavslut) för att kunna markera dessa med stjärn-ikon i artikel-dialogen
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static List<view_ContractRow> GetClosedAndRewrittenContractRows(string customer)
        {
            List<view_ContractRow> list = new List<view_ContractRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = @"SELECT [Contract_id] ,[Customer] ,[Article_number], [Offer_number] ,[License] ,[Maintenance] ,
                                        [Delivery_date] ,[Created] ,[Updated] ,[Rewritten] ,[New] ,[Removed] ,[Closure_date], [Fixed_price], 
                                        CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp, [Alias] FROM qry_ClosedOrRewrittenContractRow WHERE " + "Customer = @customer Order By " + GetOrderBy();
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

        internal void UpdateContractRowAsRemoved()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = @"UPDATE [dbo].[A_avtalsrader] SET [borttag] = 1, [RemovedFromContractId] = @removed_from_contract_id WHERE [Avtalsid] = @contract_id AND [Kund] = @customer AND [Artnr] = @article_number";
                command.Prepare();
                command.Parameters.AddWithValue("@customer", this.Customer);
                command.Parameters.AddWithValue("@contract_id", this.Contract_id);
                command.Parameters.AddWithValue("@article_number", this.Article_number);
                command.Parameters.AddWithValue("@removed_from_contract_id", this.RemovedFromContractId);

                command.ExecuteNonQuery();
            }
        }

        internal void UpdateContractRowAsRewritten()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = @"UPDATE [dbo].[A_avtalsrader] SET [borttag] = 0, [ny] = 0, [omskriv] = 1, [RemovedFromContractId] = NULL WHERE [Avtalsid] = @contract_id AND [Kund] = @customer AND [Artnr] = @article_number";
                command.Prepare();
                command.Parameters.AddWithValue("@customer", this.Customer);
                command.Parameters.AddWithValue("@contract_id", this.Contract_id);
                command.Parameters.AddWithValue("@article_number", this.Article_number);

                command.ExecuteNonQuery();
            }
        }
    }
}


