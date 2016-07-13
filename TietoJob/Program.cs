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
                    Dictionary<String,Dictionary<>>
                    List<view_Contract> contracts = view_Contract.GetContracts()
                    decimal totalValue = 0;
                    view_Contract lastContract = null;
                    int count = 1;
                    foreach (view_Contract contract in contracts)
                    {
                        if (lastContract != null && contract.Status == "Giltigt" && contract.Valid_from.HasValue
                        && contract.Valid_through.HasValue && lastContract.Valid_from.HasValue)
                        {
                            if (lastContract.Valid_from.Value.Year != contract.Valid_from.Value.Year)
                            {
                                update(customer._ID.ToString(), totalValue, lastContract.Valid_from.Value);
                                totalValue = 0;
                            }

                            foreach (view_ContractRow row in contract._ContractRows)
                            {
                                totalValue += row.License ?? 0;
                                totalValue += (row.Maintenance ?? 0) * (contract.Valid_through - contract.Valid_from).Value.Days / 30;
                            }
                            foreach (view_ContractConsultantRow row in contract._ContractConsultantRows)
                            {
                                totalValue += row.Total_price ?? 0;
                            }
                            if(count == contracts.Count)
                            {
                                update(customer._ID.ToString(), totalValue, lastContract.Valid_from.Value);
                                totalValue = 0;
                            }
                        }
                        lastContract = contract;
                        count++;
                    }
            /*}
            catch (Exception e)
            {
                throw e;
                System.Diagnostics.EventLog eventlog = new System.Diagnostics.EventLog("Job", ".", "Tieto");
                eventlog.WriteEntry(e.Message);
            }*/
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
