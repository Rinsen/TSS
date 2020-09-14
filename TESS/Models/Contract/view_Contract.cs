using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace TietoCRM.Models
{
    public enum ContractType
    {
        MainContract,
        SupplementaryContract,
        ServiceContract,
        ModuleTermination
    }
    public class view_Contract : HashtagDocument
    {
        public int _ID { get { return base._ID; } set { base._ID = value; } }

        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private String contract_type;
        public String Contract_type { get { return contract_type; } set { contract_type = value; } }

        private short? term_of_notice;
        public short? Term_of_notice { get { return term_of_notice; } set { term_of_notice = value ?? 0; } }
        /// <summary>
        ///     Returns Terms_of_notice with months include in the string
        /// </summary>
        /// <returns>Term_of_notice + "months(s)"</returns>
        public String getStringTON() { return Term_of_notice + " month(s)"; }

        private double? extension;
        public double? Extension { get { return extension; } set { extension = value ?? 0; } }

        private String status;
        public String Status { get { return status; } set { status = value; } }

        private int? crm_id;
        public int? CRM_id { get { return crm_id; } set { crm_id = value; } }

        private DateTime? valid_from;
        public DateTime? Valid_from { get { return valid_from; } set { valid_from = value; } }

        private DateTime? valid_through;
        public DateTime? Valid_through { get { return valid_through; } set { valid_through = value; } }

        private String main_contract_id;
        public String Main_contract_id { get { return main_contract_id; } set { main_contract_id = value; } }

        /// <summary>
        ///     Returns Extension with months include in the string
        /// </summary>
        /// <returns>Extension + "months(s)"</returns>
        public String getStringExtension() { return Extension + " month(s)"; }

        private DateTime? expire;
        public DateTime? Expire { get { return expire; } set { expire = value; } }

        private DateTime? observation;
        public DateTime? Observation { get { return observation; } set { observation = value; } }

        private String note;
        public String Note { get { return note; } set { note = value; } }

        private String contact_person;
        public String Contact_person { get { return contact_person; } set { contact_person = value; } }

        private DateTime? created;
        public DateTime? Created { get { return created; } set { created = value; } }

        private DateTime? updated;
        public DateTime? Updated { get { return updated; } set { updated = value; } }

        private DateTime? option_date;
        public DateTime? Option_date { get { return option_date; } set { option_date = value; } }

        private String sign;
        public String Sign { get { return sign; } set { sign = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private String resigned_contract;
        public String Resigned_contract { get; set; }

        private int summera;
        public int Summera { get; set; }

        private long ssma_timestamp;
        public long SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        private List<view_ContractRow> _contractRows;
        public List<view_ContractRow> _ContractRows { get { return _contractRows; } set { _contractRows = value; } }

        private List<view_ContractConsultantRow> _contractConsultantRows;
        public List<view_ContractConsultantRow> _ContractConsultantRows { get { return _contractConsultantRows; } set { _contractConsultantRows = value; } }

        private List<view_ContractOption> _contractOptions;
        public List<view_ContractOption> _ContractOptions { get { return _contractOptions; } set { _contractOptions = value; } }

        public List<view_CustomerProductRow> _CustomerProductRows { get; set; }
        public IEnumerable<Dictionary<String, object>> _CustomerProductRowsSorted { get; set; }
        public List<view_Module> _MatchedModules { get; set; }
        public List<String> _OrderedSystemNames { get; set; }


        public view_Contract()
            : base("Contract")
        {
            //ctr
        }

        public view_Contract(String condition)
            : base("Contract")
        {
            this.Select(condition);
        }

        public override bool Select(string condition)
        {
            bool r = base.Select(condition);
            if (r)
            { 
                this._ContractRows = view_ContractRow.GetAllContractRows(this.Contract_id, this.Customer);
                this._ContractConsultantRows = view_ContractConsultantRow.GetAllContractConsultantRow(this.Contract_id, this.Customer);
                this._ContractOptions = view_ContractOption.getAllOptions(this.Contract_id, this.Customer);
            }

            return r;
        }

        /// <summary>
        /// Get all contract of a specific customer
        /// </summary>
        /// <param name="customer">The customer to get contracts from</param>
        /// <returns>A list of strings with customer names</returns>
        public static List<view_Contract> GetContracts(String customer, bool noRows = false)
        {
            List<view_Contract> list = new List<view_Contract>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [ID], [Contract_id] ,[Customer], [Title] ,[Contract_type] ,[Term_of_notice] ,[Extension] ,[Status], [CRM_id] ,[Valid_from] ,[Valid_through] ,";
                command.CommandText += "[Main_contract_id] ,[Expire] ,[Observation] ,[Note] ,[Contact_person] ,[Created] ,[Updated] ,";
                command.CommandText += "[Option_date] ,[Sign], Area, Resigned_contract, Summera, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "Contract WHERE " + "Customer = @customer";
                //command.CommandText = "SELECT * FROM " + databasePrefix + "Contract WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 1;
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_Contract t = new view_Contract();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }
                            if (noRows == false)
                            {
                                t._ContractRows = view_ContractRow.GetAllContractRows(t.Contract_id, t.Customer);
                                t._ContractConsultantRows = view_ContractConsultantRow.GetAllContractConsultantRow(t.Contract_id, t.Customer);
                                t._ContractOptions = view_ContractOption.getAllOptions(t.Contract_id, t.Customer);
                            }
                            t.GetHashtags();
                            list.Add(t);
                        }
                    }
                }


            }
            return list;
        }

        /// <summary>
        /// Used to check what kind of contract this contract is.
        /// </summary>
        /// <param name="contractType">The type to compare to.</param>
        /// <returns>true/false depending on the compare value.</returns>
        public bool Is(ContractType contractType)
        {
            if (this.Contract_type == null)
            {
                return false;
            }
            else if(Regex.IsMatch(this.Contract_type, "Huvudavtal", RegexOptions.IgnoreCase) 
                && contractType == ContractType.MainContract)
            {
                return true;
            }
            else if (Regex.IsMatch(this.Contract_type, "Tilläggsavtal", RegexOptions.IgnoreCase) 
                && contractType == ContractType.SupplementaryContract)
            {
                return true;
            }
            else if (Regex.IsMatch(this.Contract_type, "Tjänsteavtal", RegexOptions.IgnoreCase)
                && contractType == ContractType.ServiceContract)
            {
                return true;
            }
            else if (Regex.IsMatch(this.Contract_type, "Modulavslut", RegexOptions.IgnoreCase)
                && contractType == ContractType.ModuleTermination)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_Contract p = obj as view_Contract;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Contract_id == p.Contract_id) && (this.Customer == p.Customer);
        }

        /// <summary>
        /// Gets all contracts.
        /// </summary>
        /// <returns>List of contracts.</returns>
        public static List<view_Contract> GetContracts(bool noRows = false)
        {
            List<view_Contract> list = new List<view_Contract>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT [ID], [Contract_id] ,[Customer], [Title] ,[Contract_type] ,
                                        [Term_of_notice] ,[Extension] ,[Status], [CRM_id] ,[Valid_from] ,
                                        [Valid_through] ,[Main_contract_id] ,[Expire] ,[Observation] ,[Note] ,
                                        [Contact_person] ,[Created] ,[Updated] ,[Option_date] ,[Sign], Area, 
                                        Resigned_contract, Summera, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "Contract";

                command.Prepare();
                command.ExecuteNonQuery();

                List<view_ContractRow> cRows = view_ContractRow.GetAllContractRows();
                List<view_ContractConsultantRow> ccRows = view_ContractConsultantRow.GetAllContractConsultantRow();
                List<view_ContractOption>  cOptions = view_ContractOption.getAllOptions();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_Contract t = new view_Contract();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }
                            if (noRows == false)
                            {
                                t._ContractRows = cRows.Where(o => o.Contract_id == t.Contract_id && o.Customer == t.Customer).ToList();
                                t._ContractConsultantRows = ccRows.Where(o => o.Contract_id == t.Contract_id && o.Customer == t.Customer).ToList();
                                t._ContractOptions = cOptions.Where(o => o.Contract_id == t.Contract_id && o.Customer == t.Customer).ToList();
                            }
                            t.GetHashtags();
                            list.Add(t);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets all valid contracts of a client that the specific user is a representative of.
        /// </summary>
        /// <param name="sign">The sign of the user</param>
        /// <param name="ctrStatus">The status of the Contract</param>
        /// <returns>A list of valid contracts.</returns>
        public static List<view_Contract> GetContractsByStatus(String sign, String ctrStatus)
        {
            List<view_Contract> list = new List<view_Contract>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT vc.[ID], vc.[Contract_id] ,vc.[Customer], vc.[Title] ,vc.[Contract_type] ,
                                        vc.[Term_of_notice] ,vc.[Extension] ,vc.[Status] ,vc.[Valid_from] ,vc.[Valid_through] ,
                                        vc.[Main_contract_id] ,vc.[Expire] ,vc.[Observation] ,vc.[Note] ,vc.[Contact_person] ,vc.[Created] ,
                                        vc.[Updated] ,vc.[Option_date], vc.[Sign], vc.[Area], Resigned_contract, vc.[Summera], CAST(vc.SSMA_timestamp AS BIGINT) AS SSMA_timestamp
                                        FROM " + databasePrefix + "Contract as vc, " + databasePrefix + 
                                        @"Customer as cus where cus.Representative = @sign and vc.Customer = cus.Customer and 
                                        vc.status = " + ctrStatus + " and vc.Contract_type = 'huvudavtal'";

                command.Prepare();
                command.Parameters.AddWithValue("@sign", sign);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 1;
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {

                            view_Contract t = new view_Contract();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }
                            t._ContractRows = view_ContractRow.GetAllContractRows(t.Contract_id, t.Customer);
                            t._ContractConsultantRows = view_ContractConsultantRow.GetAllContractConsultantRow(t.Contract_id, t.Customer);
                            t._ContractOptions = view_ContractOption.getAllOptions(t.Contract_id, t.Customer);
                            list.Add(t);
                            count++;

                        }
                    }
                }


            }
            return list;
        }
        /// <summary>
        /// Check if any kind of contract exists for a specific customer
        /// </summary>
        /// <param name="customer">The customer to get contracts from</param>
        /// <returns>A list of strings with customer names</returns>
        public static bool CustomerContractExists(String customer)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = "SELECT [ID] FROM " + databasePrefix + "Contract WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
        }
        public decimal? ContractMaintenanceSum()
        {
            decimal? mainSum = 0;

            foreach(view_ContractRow r in this._contractRows)
            {
                mainSum += r.Maintenance;
            }
            return mainSum;
        }
        public decimal? ContractLicenseSum()
        {
            decimal? mainSum = 0;

            foreach (view_ContractRow r in this._contractRows)
            {
                mainSum += r.License;
            }
            return mainSum;
        }
        public decimal? ContractServiceSum()
        {
            decimal? mainSum = 0;

            foreach (view_ContractConsultantRow r in this._contractConsultantRows)
            {
                mainSum += r.Total_price;
            }
            return mainSum;
        }
    }

}