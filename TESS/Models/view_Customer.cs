using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace TietoCRM.Models
{
   public class view_Customer : SQLBaseClass
	{
        private static SelectOptions<view_Customer> selectOptions = new SelectOptions<view_Customer>();
        public static SelectOptions<view_Customer> _SelectOptions { get { return selectOptions; } }

        private int id;
        public int _ID { get { return id; } set { id = value; } }

        private String customer;
		public String Customer { get{ return customer; } set{ customer = value; } }

        private String representative;
        public String Representative { get { return representative; } set { representative = value; } }

        private String short_name;
		public String Short_name { get{ return short_name; } set{ short_name = value; } }

		private String customer_type;
		public String Customer_type { get{ return customer_type; } set{ customer_type = value; } }

		private String address;
		public String Address { get{ return address; } set{ address = value; } }

		private String zip_code;
		public String Zip_code { get{ return zip_code; } set{ zip_code = value; } }

		private String city;
		public String City { get{ return city; } set{ city = value; } }

		private String telephone;
		public String Telephone { get{ return telephone; } set{ telephone = value; } }

		private String fax;
		public String Fax { get{ return fax; } set{ fax = value; } }

		private String web_address;
		public String Web_address { get{ return web_address; } set{ web_address = value; } }

		private String corporate_identity_number;
		public String Corporate_identity_number { get{ return corporate_identity_number; } set{ corporate_identity_number = value; } }

		private String email_format;
		public String Email_format { get{ return email_format; } set{ email_format = value; } }

		private short? county;
		public short? County { get{ return county ?? 0; } set{ county = value; } }
        public String GetCounty()
        {
            return (selectOptions.GetOptions("County").Find(s => s.Value == this.County.ToString()).Text ?? this.County.ToString()).ToString();
        }

		private short? municipality;
		public short? Municipality { get{ return municipality; } set{ municipality = value; } }

		private String it_manager;
		public String IT_manager { get{ return it_manager; } set{ it_manager = value; } }

		private String it_manager_telephone;
		public String IT_manager_telephone { get{ return it_manager_telephone; } set{ it_manager_telephone = value; } }

		private String it_manager_mobile;
		public String IT_manager_mobile { get{ return it_manager_mobile; } set{ it_manager_mobile = value; } }

		private String it_manager_email;
		public String IT_manager_email { get{ return it_manager_email; } set{ it_manager_email = value; } }

		private String ea_system;
		public String EA_system { get{ return ea_system; } set{ ea_system = value; } }

		private String pa_system;
		public String PA_system { get{ return pa_system; } set{ pa_system = value; } }

		private String other_1;
		public String Other_1 { get{ return other_1; } set{ other_1 = value; } }

		private String other_2;
		public String Other_2 { get{ return other_2; } set{ other_2 = value; } }

		private bool? pul;
		public bool? PUL { get{ return pul; } set{ pul = value; } }

		private String note;
		public String Note { get{ return note; } set{ note = value; } }

		private int? inhabitant_level;
		public int? Inhabitant_level { get{ return inhabitant_level; } set{ inhabitant_level = value; } }

		private long ssma_timestamp;
		public long SSMA_timestamp { get{ return ssma_timestamp; } set{ ssma_timestamp = value; } }

        private List<String> representatives;
        public List<String> _Representatives { get { return representatives; } set { representatives = value; } }

        public String GetReprensentativesAsString()
        {
            String reps = "";
            int count = 0;
            foreach (String rep in this._Representatives)
            {
                count++;
                if (!String.IsNullOrEmpty(rep) && count < (this._Representatives.Count - 1))
                    reps += rep + ", ";
                else if (count < this._Representatives.Count)
                    reps += rep + " and ";
                else
                    reps += rep;
            }
            return reps;
        }

        public view_Customer() : base("Customer")
		{
            this._Representatives = new List<String>();
        }

        public view_Customer(String condition) : base("Customer")
        {
            this.Select(condition);
        }

        public override bool Select(String condition)
        {
            bool returnVal = base.Select(condition);
            if (returnVal)
            {
                this._Representatives = this.GetCustomerRepresentatives();
            }
            else
                this._Representatives = new List<String>();

            return returnVal;
        }

        public override void Update(string condition)
        {
            if(this._Representatives.Count > 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String deleteQuery = "DELETE FROM " + databasePrefix + "CustomerDivision WHERE CustomerID=@id";

                    SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);

                    deleteCommand.Prepare();
                    deleteCommand.Parameters.AddWithValue("@id", this._ID);
                    deleteCommand.ExecuteNonQuery();

                    foreach (String rep in _Representatives)
                    {
                        if(rep != null)
                        {
                            String insertQuery = "INSERT INTO " + databasePrefix + "CustomerDivision (CustomerID,Representative) VALUES(@customerid,@rep)";

                            SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

                            insertCommand.Prepare();
                            insertCommand.Parameters.AddWithValue("@customerid", this._ID);
                            insertCommand.Parameters.AddWithValue("@rep", rep);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            base.Update(condition);
        }

        public void Insert(List<String> representatives)
        {
            base.Insert();
            base.Select("Customer = '" + this.Customer + "' AND Short_name = '" + this.Short_name + "'");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (String rep in representatives)
                {
                    String insertQuery = "INSERT INTO " + databasePrefix + "CustomerDivision (CustomerID,Representative) VALUES(@customerid,@rep)";

                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

                    insertCommand.Prepare();
                    insertCommand.Parameters.AddWithValue("@customerid", this._ID);
                    insertCommand.Parameters.AddWithValue("@rep", rep);
                    insertCommand.ExecuteNonQuery();
                }
            }
            
        }

        public override void Delete(string condition)
        {
            if (this._Representatives.Count > 0)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String deleteQuery = "DELETE FROM " + databasePrefix + "CustomerDivision WHERE CustomerID=@id";

                    SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);

                    deleteCommand.Prepare();
                    deleteCommand.Parameters.AddWithValue("@id", this._ID);
                    deleteCommand.ExecuteNonQuery();
                }
            }
            base.Delete(condition);
        }

        private List<String> GetCustomerRepresentatives()
        {
            List<String> list = new List<String>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT Representative FROM " + databasePrefix + "CustomerDivision WHERE CustomerID=@id";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                command.Parameters.AddWithValue("@id", this._ID);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            return list;
        }

        public void SetRepresentative(String representative)
        {
            this.Representative = representative;
        }
        /// <summary>
        /// update the customer information in all tables when changed.
        /// </summary>
        /// <param name="customer">The customer name</param>
        public void SetCustomer(String customer)
        {
            foreach(view_CustomerContact contact in view_CustomerContact.getAllCustomerContacts(this.Customer))
            {
                contact.Customer = customer;
                contact.Update("Customer = '" + this.Customer + "' AND Contact_person = '" + contact.Contact_person + "' AND Email = '" + contact.Email + "'");
            }
            foreach(view_CustomerOffer offer in view_CustomerOffer.getAllCustomerOffers(this.Customer))
            {
                offer.Customer = customer;
                offer.Update("ssma_timestamp = " + offer.SSMA_timestamp);
            }
            foreach(view_Contract contract in view_Contract.GetContracts(this.Customer))
            {
                contract.Customer = customer;
                foreach(view_ContractRow row in contract._ContractRows)
                {
                    row.Customer = customer;
                    row.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + row.Contract_id + "' AND Article_number = " + row.Article_number);
                }
                foreach (view_ContractOption option in contract._ContractOptions)
                {
                    option.Customer = customer;
                    option.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + option.Contract_id + "' AND Article_number = " + option.Article_number);
                }
                foreach (view_ContractConsultantRow cRow in contract._ContractConsultantRows)
                {
                    cRow.Customer = customer;
                    cRow.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + cRow.Contract_id + "' AND Code = " +  cRow.Code);
                }
                contract.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + contract.Contract_id + "'");
            }

            foreach(view_ContractTemplate template in view_ContractTemplate.GetAllContractTemplates(this.Customer))
            {
                template.Customer = customer;
                template.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + template.Contract_id + "'");
            }

            foreach (view_ContractText text in view_ContractText.GetAllContractTexts(this.Customer))
            {
                text.Customer = customer;
                text.Update("Customer = '" + this.Customer + "' AND Contract_id = '" + text.Contract_id + "'");
            }

            String oldCustomer = this.Customer;
            this.Customer =  customer;
            this.Update("Customer = '" + oldCustomer + "'");
        }

        /// <summary>
        /// Gets all customers in the database.
        /// </summary>
        /// <returns>A list of all a customers.</returns>
        public static List<view_Customer> getAllCustomers()
        {
            return getAllCustomers(null);
        }

        public static List<view_Customer> getAllCustomers(String representive)
        {
            List<view_Customer> list = new List<view_Customer>();
            foreach(String id in GetCustomerIds(representive))
            {
                view_Customer c = new view_Customer("ID=" + id);
                list.Add(c);
            }
            return list;
        }

        /// <summary>
        /// Get the names of all customers
        /// </summary>
        /// <returns>A list of customer names</returns>
        public static List<String> getCustomerNames(String representive)
        {
            List<String> list = new List<String>();
            List<String> customerIds = GetCustomerIds(representive);
            foreach (String id in customerIds)
            {
                list.Add((new view_Customer("ID=" + id)).Customer);
            }
            return list;
        }

        /// <summary>
        /// Get the names of all customers with a specific representative
        /// </summary>
        /// <param name="representative">The sign of the representative.</param>
        /// <returns>A list of customer names</returns>
        public static List<String> getCustomerNames()
        {
            return getCustomerNames(null);
        }

        public static List<String> GetCustomerIds(String representive)
        {
            List<String> list = new List<String>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String condition = "";
                if (representive != null)
                    condition = "WHERE Representative = @representive";

                String query = "SELECT DISTINCT [CustomerID] FROM " + databasePrefix + "CustomerDivision " + condition;

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                if (representive != null)
                    command.Parameters.AddWithValue("@representive", representive);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        list.Add(reader.GetInt32(0).ToString());
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get the names of all customers with a specific representative
        /// </summary>
        /// <param name="representative">The sign of the representative.</param>
        /// <returns>A list of customer names</returns>
        public static List<String> GetCustomerIds()
        {
            return getCustomerNames(null);
        }



    }

}
