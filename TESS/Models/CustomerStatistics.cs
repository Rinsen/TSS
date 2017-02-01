using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public static class SqlCommandExt
    {
        /// <summary>
        /// This will add an array of parameters to a SqlCommand. This is used for an IN statement.
        /// Use the returned value for the IN part of your SQL call. (i.e. SELECT * FROM table WHERE field IN ({paramNameRoot}))
        /// </summary>
        /// <param name="cmd">The SqlCommand object to add parameters to.</param>
        /// <param name="values">The array of strings that need to be added as parameters.</param>
        /// <param name="paramNameRoot">What the parameter should be named followed by a unique value for each value. This value surrounded by {} in the CommandText will be replaced.</param>
        /// <param name="start">The beginning number to append to the end of paramNameRoot for each value.</param>
        /// <param name="separator">The string that separates the parameter names in the sql command.</param>
        public static SqlParameter[] AddArrayParameters<T>(this SqlCommand cmd, IEnumerable<T> values, string paramNameRoot, int start = 1, string separator = ", ")
        {
            /* An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually. 
             * Each item in the array will end up being it's own SqlParameter so the return value for this must be used as part of the
             * IN statement in the CommandText.
             */
            var parameters = new List<SqlParameter>();
            var parameterNames = new List<string>();
            var paramNbr = start;
            foreach (var value in values)
            {
                var paramName = string.Format("@{0}{1}", paramNameRoot, paramNbr++);
                parameterNames.Add(paramName);
                parameters.Add(cmd.Parameters.AddWithValue(paramName, value));
            }

            cmd.CommandText = cmd.CommandText.Replace("{" + paramNameRoot + "}", string.Join(separator, parameterNames));

            return parameters.ToArray();
        }
    }

    public class CustomerStatistics : Statistics
    {
        private view_Customer customer;
        public view_Customer Customer
        {
            get
            {
                return customer;
            }
        }

        private bool fetchedData = false;

        private List<Dictionary<String, Object>> perYear = null;
        /// <summary>
        /// Gets the total spent from a customer through all years
        /// </summary>
        public decimal TotalSpent
        {
            get
            {
                decimal sum = 0;
                List<Dictionary<String, Object>> list;
                if (!useCachedData && !fetchedData)
                    list = this.GetMoneyPerYear();
                else
                    list = this.perYear;

                foreach (Dictionary<String, Object> dic in list)
                {
                    sum += (decimal)dic["Total_value"];
                }

                return sum;
            }
        }
        /// <summary>
        /// Gets the total spent by a customer on a specified year
        /// </summary>
        /// <param name="year">What year to get total spent from</param>
        /// <returns></returns>
        public decimal GetTotalSpent(int year, String area)
        {
            List<Dictionary<String, Object>> list;
            if (this.perYear == null)
                this.perYear = list = this.GetMoneyPerYear();
            else
                list = this.perYear;
            view_User user = new view_User();
            user.Area = area;
            List<Dictionary<String, Object>> yearList = list.Where(d => (int)d["Year"] == year && user.IfSameArea((String)d["Area"])).ToList();
            if (yearList.Count > 0)
            {
                if (user.Area == "*")
                {
                    decimal sum = 0;
                    foreach (Dictionary<String, Object> dic in yearList)
                    {
                        sum += (decimal)dic["Total_value"];
                    }
                    return sum;
                }
                else
                    return (decimal)yearList[0]["Total_value"];
            }
            else
               throw new StatisticsException("The year (" + year + ") didnt exist in this object", this.Customer);
        }

        /// <summary>
        /// By default this class dont use cached data
        /// </summary>
        /// <param name="CustomerID">What customer ID to get statistics from</param>
        public CustomerStatistics(int CustomerID) : this(new view_Customer("ID=" + CustomerID), false) { }
        /// <summary>
        /// By default this class dont use cached data
        /// </summary>
        /// <param name="customer">What customer to get statistics from</param>
        public CustomerStatistics(view_Customer customer) : this(customer, false) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CustomerID">What customer ID to get statistics from</param>
        /// <param name="useCachedData">whether to use cached data from SQL server or generate new data</param>
        public CustomerStatistics(int CustomerID, bool useCachedData) : this(new view_Customer("ID=" + CustomerID), useCachedData) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer">What customer to get statistics from</param>
        /// <param name="useCachedData">whether to use cached data from SQL server or generate new data</param>
        public CustomerStatistics(view_Customer customer, bool useCachedData)
        {
            this.customer = customer;
            this.useCachedData = useCachedData;
            if (useCachedData)
                this.GetCachedData();
        }

        /// <summary>
        /// Calculates the Money spent per year from the objects customer and then returns it
        /// </summary>
        /// <returns> returns a list of a dictionary that represents a a year and total spent</returns>
        private List<Dictionary<String, Object>> GetMoneyPerYear()
        {
            List<Dictionary<String, Object>> customerYear = new List<Dictionary<string, object>>();
            List<view_Contract> contracts = view_Contract.GetContracts(this.Customer.Customer).OrderBy(c => c.Contract_type).ToList();
            foreach (view_Contract contract in contracts)
            {
                bool a = (contract.Is(ContractType.SupplementaryContract) && contract.Created.HasValue);
                bool b = (contract.Is(ContractType.MainContract) && contract.Valid_from.HasValue && contract.Valid_through.HasValue);
                bool c = (contract.Status == "Giltigt" || contract.Status == "Avslutat");
                if ((a || b) && c)
                {

                    view_Customer customer = new view_Customer("Customer='" + contract.Customer + "'");
                    decimal totalValue = 0;

                    foreach (view_ContractRow row in contract._ContractRows)
                    {
                        totalValue += row.License ?? 0;
                        if (contract.Valid_from.HasValue && contract.Valid_through.HasValue)
                            totalValue += (row.Maintenance ?? 0) * (contract.Valid_through - contract.Valid_from).Value.Days / 30;
                        else
                            totalValue += (row.Maintenance ?? 0) * 12;
                    }
                    foreach (view_ContractConsultantRow row in contract._ContractConsultantRows)
                    {
                        totalValue += row.Total_price ?? 0;
                    }
                   
                    Dictionary<string, object> dic;
                    if (contract.Is(ContractType.MainContract))
                        dic = GetCorrectYear(contract.Valid_from.Value.Year, contract.Area, customerYear);
                    else
                        dic = GetCorrectYear(contract.Created.Value.Year, contract.Area, customerYear);

                    if (dic == null)
                    {
                        dic = new Dictionary<string, object>();
                        dic.Add("Total_value", totalValue);
                        if (contract.Is(ContractType.MainContract))
                        {
                            dic.Add("Year", contract.Valid_from.Value.Year);
                            dic.Add("Date", contract.Valid_from.Value);
                        }
                        else
                        {
                            dic.Add("Year", contract.Created.Value.Year);
                            dic.Add("Date", contract.Created.Value);
                        }
                        customerYear.Add(dic);
                    }
                    else
                    {
                        dic["Total_value"] = (decimal)dic["Total_value"] + totalValue;
                    }
                }
            }
            this.fetchedData = true;
            return customerYear;
        }

        /// <summary>
        /// Gets the money per year from a cached data on a SQL server
        /// </summary>
        /// <returns> returns a list of a dictionary that represents a a year and total spent</returns>
        protected override bool GetCachedData()
        {
            List<Dictionary<String, Object>> list = new List<Dictionary<String, Object>>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String query = "SELECT Customer_ID, Total_value, Year, Area FROM " + "dbo.view_" + "TCVCalculator WHERE Customer_ID=@id";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                command.Parameters.AddWithValue("@id", customer._ID);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, Object> dic = new Dictionary<String, Object>();
                            int i = 1;
                            while (reader.FieldCount > i)
                            {
                                dic.Add(reader.GetName(i), reader.GetValue(i));
                                i++;
                            }
                            list.Add(dic);
                        }
                    }
                }
            }
            this.perYear = list;
            if (this.perYear.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the dictionary with matching year from the given list
        /// </summary>
        /// <param name="year">What year to search for</param>
        /// <param name="list">A list with a dictionary that represents a year with total spent that year for a customer</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetCorrectYear(int year, String area, List<Dictionary<string, object>> list)
        {
            foreach (Dictionary<string, object> dic in list)
            {
                if ((int)dic["Year"] == year && (String)dic["Area"] == area)
                    return dic;
            }

            return null;
        }


        /// <summary>
        /// Inserts all the customers money spent per year
        /// </summary>
        public static void UpdateAllToSQLServer()
        {
            Dictionary<int, List<Dictionary<String, Object>>> customers = new Dictionary<int, List<Dictionary<string, object>>>();
            List<view_Contract> contracts = view_Contract.GetContracts().OrderBy(c => c.Area).ThenBy(c => c.Contract_type).ToList();
            foreach (view_Contract contract in contracts)
            {
                bool a = (contract.Is(ContractType.SupplementaryContract) && contract.Created.HasValue);
                bool b = (contract.Is(ContractType.MainContract) && contract.Valid_from.HasValue && contract.Valid_through.HasValue);
                bool c = (contract.Status == "Giltigt" || contract.Status == "Avslutat");
//Om kontraktet är tilläggsavtal (varför saknas tjänsteavtal?) eller huvudavtal samt är av status Giltigt eller Avslutat
                if ((a || b) && c)
                {
                    view_Customer customer = new view_Customer();
                    if(customer.Select("Customer='" + contract.Customer + "'"))
                    {
                        decimal totalValue = 0;
                        foreach (view_ContractRow row in contract._ContractRows)
                        {
                            totalValue += row.License ?? 0;
                            if (contract.Valid_from.HasValue && contract.Valid_through.HasValue)
                                totalValue += (row.Maintenance ?? 0) * (contract.Valid_through - contract.Valid_from).Value.Days / 30;
                            else
                                totalValue += (row.Maintenance ?? 0) * 12;
                        }
                        foreach (view_ContractConsultantRow row in contract._ContractConsultantRows)
                        {
                            totalValue += row.Total_price ?? 0;
                        }
                        if (!customers.Keys.Contains(customer._ID))
                        {
                            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                            list.Add(new Dictionary<string, object>());
                            customers.Add(customer._ID, list);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("Total_value", totalValue);
                            if (contract.Is(ContractType.MainContract))
                            {
                                customers[customer._ID][customers[customer._ID].Count - 1].Add("Year", contract.Valid_from.Value.Year);
                                customers[customer._ID][customers[customer._ID].Count - 1].Add("Date", contract.Valid_from.Value);
                            }
                            else
                            {
                                customers[customer._ID][customers[customer._ID].Count - 1].Add("Year", contract.Created.Value.Year);
                                customers[customer._ID][customers[customer._ID].Count - 1].Add("Date", contract.Created.Value);
                            }
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("Area", contract.Area);
                        }
                        else
                        {
                            Dictionary<string, object> dic;
                            if (contract.Is(ContractType.MainContract))
                                dic = GetCorrectYear(contract.Valid_from.Value.Year, contract.Area, customers[customer._ID]);
                            else
                                dic = GetCorrectYear(contract.Created.Value.Year, contract.Area, customers[customer._ID]);

                            if (dic == null)
                            {
                                dic = new Dictionary<string, object>();
                                dic.Add("Total_value", totalValue);
                                if (contract.Is(ContractType.MainContract))
                                {
                                    dic.Add("Year", contract.Valid_from.Value.Year);
                                    dic.Add("Date", contract.Valid_from.Value);
                                }
                                else
                                {
                                    dic.Add("Year", contract.Created.Value.Year);
                                    dic.Add("Date", contract.Created.Value);
                                }
                                dic.Add("Area", contract.Area);
                                customers[customer._ID].Add(dic);
                            }
                            else
                            {
                                dic["Total_value"] = (decimal)dic["Total_value"] + totalValue;
                            }
                        }
                    }
                }
            }
            Truncate();
            foreach (KeyValuePair<int, List<Dictionary<String, Object>>> keyVal in customers)
            {
                foreach (Dictionary<String, Object> dic in keyVal.Value)
                {
                    Insert(keyVal.Key.ToString(), (decimal)dic["Total_value"], (DateTime)dic["Date"], (String)dic["Area"]);
                }
            }
        }
        /// <summary>
        /// Inserts a given customer with a year and total spent 
        /// </summary>
        /// <param name="customerId">The id of the customer</param>
        /// <param name="totalValue">The total spent of the customer</param>
        /// <param name="year">what year it is for</param>
        /// <param name="area">what area it is for</param>
        private static void Insert(String customerId, decimal totalValue, DateTime year, String area)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String insertQuery = "INSERT INTO " + "dbo.view_" + "TCVCalculator (Customer_ID,Total_value,Year,Area) VALUES(@customerid,@val,@year,@area)";

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

                insertCommand.Prepare();
                insertCommand.Parameters.AddWithValue("@customerid", customerId);
                insertCommand.Parameters.AddWithValue("@val", totalValue);
                insertCommand.Parameters.AddWithValue("@year", year.Year);
                insertCommand.Parameters.AddWithValue("@area", area);
                insertCommand.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Removes all rows from the sql table
        /// </summary>
        private static void Truncate()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();
                String truncateQuery = "TRUNCATE TABLE " + "dbo." + "TCV_kalkyl";

                SqlCommand TruncateCommande = new SqlCommand(truncateQuery, connection);

                TruncateCommande.Prepare();
                TruncateCommande.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates the current customers statistics to the SQL server
        /// </summary>
        public override void UpdateToSQLServer()
        {
            this.perYear = this.GetMoneyPerYear();

            foreach(Dictionary<String,Object> dic in this.perYear)
            {
                Insert(Customer._ID.ToString(), (decimal)dic["Total_spent"], (DateTime)dic["Year"], (String)dic["Area"]);
            }
                
        }

        public static List<CustomerStatistics> GetAllCustomerStatstics(List<view_Customer> customers, int? year)
        {
            List<CustomerStatistics> list = new List<CustomerStatistics>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String whereQuery = "";
                if (year != null)
                    whereQuery = " AND Year=@year";

                String query = "SELECT Customer_ID, Total_value, Year, Area FROM " + "dbo.view_" + "TCVCalculator WHERE Customer_ID IN ({cid})" + whereQuery;

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                command.AddArrayParameters(customers.Select(c => c._ID), "cid");

                if (year != null)
                    command.Parameters.AddWithValue("@year", year);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            Dictionary<String, Object> dic = new Dictionary<String, Object>();
                            int id = reader.GetInt32(0);
                            int i = 1;
                            while (reader.FieldCount > i)
                            {
                                dic.Add(reader.GetName(i), reader.GetValue(i));

                                i++;
                            }
                            view_Customer customer = new view_Customer();
                            customer = customers.Find(c => c._ID == id);
                            CustomerStatistics cs;
                            if (!list.Any(css => css.Customer._ID == id))
                            {
                                cs = new CustomerStatistics(customer);
                                cs.perYear = new List<Dictionary<string, object>>();
                                cs.perYear.Add(dic);
                                list.Add(cs);
                            }
                            else
                            {
                                cs = list.Find(c => c.Customer._ID == id);
                                cs.perYear.Add(dic);
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}