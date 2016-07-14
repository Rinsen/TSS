using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TietoCRM.Models;

namespace TietoJob
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!System.Diagnostics.EventLog.SourceExists("Tieto"))
                System.Diagnostics.EventLog.CreateEventSource("Tieto", "Job");
            try
            {
                CustomerStatistics.UpdateAllToSQLServer();
                UserStatistics.UpdateAllToSQLServer();

            }
            catch (Exception e)
            {
                System.Diagnostics.EventLog eventlog = new System.Diagnostics.EventLog("Job", ".", "Tieto");
                eventlog.WriteEntry(e.Message);
                throw e;
            }
        }


        private static void UpdateMoneyPerYearCustomer()
        {
            Dictionary<int, List<Dictionary<String, Object>>> customers = new Dictionary<int, List<Dictionary<string, object>>>();
            List<view_Contract> contracts = view_Contract.GetContracts().OrderBy(c => c.Contract_type).ToList();
            foreach (view_Contract contract in contracts)
            {
                bool a = (contract.Is(ContractType.SupplementaryContract) && contract.Created.HasValue);
                bool b = (contract.Is(ContractType.MainContract) && contract.Valid_from.HasValue && contract.Valid_through.HasValue);
                if (a || b)
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
                    if (!customers.Keys.Contains(customer._ID))
                    {
                        List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                        list.Add(new Dictionary<string, object>());
                        customers.Add(customer._ID, list);
                        customers[customer._ID][customers[customer._ID].Count - 1].Add("value", totalValue);
                        if (contract.Is(ContractType.MainContract))
                        {
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("year", contract.Valid_from.Value.Year);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("date", contract.Valid_from.Value);
                        }
                        else
                        {
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("year", contract.Created.Value.Year);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("date", contract.Created.Value);
                        }
                    }
                    else
                    {
                        Dictionary<string, object> dic;
                        if (contract.Is(ContractType.MainContract))
                            dic = GetCorrectYear(contract.Valid_from.Value.Year, customers[customer._ID]);
                        else
                            dic = GetCorrectYear(contract.Created.Value.Year, customers[customer._ID]);

                        if (dic == null)
                        {
                            dic = new Dictionary<string, object>();
                            dic.Add("value", totalValue);
                            if (contract.Is(ContractType.MainContract))
                            {
                                dic.Add("year", contract.Valid_from.Value.Year);
                                dic.Add("date", contract.Valid_from.Value);
                            }
                            else
                            {
                                dic.Add("year", contract.Created.Value.Year);
                                dic.Add("date", contract.Created.Value);
                            }
                            customers[customer._ID].Add(dic);
                        }
                        else
                        {
                            dic["value"] = (decimal)dic["value"] + totalValue;
                        }
                    }
                }
            }

            foreach (KeyValuePair<int, List<Dictionary<String, Object>>> keyVal in customers)
            {
                foreach (Dictionary<String, Object> dic in keyVal.Value)
                {
                    update(keyVal.Key.ToString(), (decimal)dic["value"], (DateTime)dic["date"]);
                }
            }
        }

        private static Dictionary<string, object> GetCorrectYear(int year, List<Dictionary<string, object>> list)
        {
            foreach(Dictionary<string, object> dic in list)
            {
                if ((int)dic["year"] == year)
                    return dic;
            }

            return null;
        }

        private static void update(String customerId, decimal totalValue, DateTime year)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();
                String deleteQuery = "DELETE FROM " + "dbo.view_" + "TCVCalculator WHERE Customer_ID=@customerid AND Year=@year";

                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);

                deleteCommand.Prepare();
                deleteCommand.Parameters.AddWithValue("@customerid", customerId);
                deleteCommand.Parameters.AddWithValue("@year", year.ToString("yyyy-MM-dd"));
                deleteCommand.ExecuteNonQuery();

                String insertQuery = "INSERT INTO " + "dbo.view_" + "TCVCalculator (Customer_ID,Total_value,Year) VALUES(@customerid,@val,@year)";

                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

                insertCommand.Prepare();
                insertCommand.Parameters.AddWithValue("@customerid", customerId);
                insertCommand.Parameters.AddWithValue("@val", totalValue);
                insertCommand.Parameters.AddWithValue("@year", year.ToString("yyyy-MM-dd"));
                insertCommand.ExecuteNonQuery();
            }
        }
    }
}
