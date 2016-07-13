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
            //try
            //{
            Dictionary<int, List<Dictionary<String, Object>>> customers = new Dictionary<int, List<Dictionary<string, object>>>();
            List<view_Contract> contracts = view_Contract.GetContracts().OrderBy(c => c.Contract_type).ToList();
            foreach (view_Contract contract in contracts)
            {
                if(contract.Is(ContractType.MainContract) || contract.Is(ContractType.SupplementaryContract))
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
                    if (contract.Valid_from.HasValue && contract.Valid_through.HasValue && contract.Is(ContractType.MainContract))
                    {
                        if (!customers.Keys.Contains(customer._ID))
                        {
                            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                            list.Add(new Dictionary<string, object>());
                            customers.Add(customer._ID, list);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("value", totalValue);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("year", contract.Valid_from.Value.Year);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("date", contract.Valid_from.Value);
                            HashSet<String> mainContracts = new HashSet<string>();
                            mainContracts.Add(contract.Main_contract_id);
                            customers[customer._ID][customers[customer._ID].Count - 1].Add("mainContracts", mainContracts);
                        }
                        else
                        {
                            Dictionary<string, object> dic = GetCorrectYear(contract.Valid_from.Value.Year, customers[customer._ID]);
                            if (dic == null)
                            {
                                dic = new Dictionary<string, object>();
                                dic.Add("value", totalValue);
                                dic.Add("year", contract.Valid_from.Value.Year);
                                dic.Add("date", contract.Valid_from.Value);
                                HashSet<String> mainContracts = new HashSet<string>();
                                mainContracts.Add(contract.Main_contract_id);
                                dic.Add("mainContracts", mainContracts);
                                customers[customer._ID].Add(dic);
                            }
                            else
                            {
                                dic["value"] = (decimal)dic["value"] + totalValue;
                                ((HashSet<String>)dic["mainContracts"]).Add(contract.Main_contract_id);

                            }
                        }
                    }
                    else // Must be SupplementaryContract
                    {
                        if (customers.Keys.Contains(customer._ID))
                        {
                            foreach (Dictionary<String, Object> dic in customers[customer._ID])
                            {
                                if (((HashSet<String>)dic["mainContracts"]).Contains(contract.Main_contract_id))
                                {
                                    dic["value"] = (decimal)dic["value"] + totalValue;
                                }
                            }
                        }
                    }
                }      
            }

            foreach(KeyValuePair<int,List<Dictionary<String, Object>>> keyVal in customers)
            {
                foreach(Dictionary<String, Object> dic in keyVal.Value)
                {
                    update(keyVal.Key.ToString(), (decimal)dic["value"], (DateTime)dic["date"]);
                }
            }
            /*}
            catch (Exception e)
            {
                throw e;
                System.Diagnostics.EventLog eventlog = new System.Diagnostics.EventLog("Job", ".", "Tieto");
                eventlog.WriteEntry(e.Message);
            }*/
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
