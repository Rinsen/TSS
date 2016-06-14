using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace TietoCRM.Models
{
   public class view_Customer : SQLBaseClass
	{
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
		public short? County { get{ return county; } set{ county = value; } }

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

		public view_Customer() : base("Customer")
		{
			//ctr
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
            List<view_Customer> list = new List<view_Customer>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [Customer] ,[Representative] ,[Short_name] ,[Customer_type] ,[Address] ,[Zip_code] ,[City] ,[Telephone] ,[Fax] ,[Web_address] ,[Corporate_identity_number] ,[Email_format] ,[County] ,[Municipality] ,[IT_manager] ,[IT_manager_telephone] ,[IT_manager_mobile] ,[IT_manager_email] ,[EA_system] ,[PA_system] ,[Other_1] ,[Other_2] ,[PUL] ,[Note] ,[Inhabitant_level] FROM " + databasePrefix + "Customer";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
        
                    while (reader.Read())
                    {
                        view_Customer k = new view_Customer();
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            k.SetValue(k.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(k);
                    }
                }
            }
            return list;
        }

        public static List<view_Customer> getAllCustomers(String representive)
        {
            List<view_Customer> list = new List<view_Customer>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [Customer] ,[Representative] ,[Short_name] ,[Customer_type] ,[Address] ,[Zip_code] ,[City] ,[Telephone] ,[Fax] ,[Web_address] ,[Corporate_identity_number] ,[Email_format] ,[County] ,[Municipality] ,[IT_manager] ,[IT_manager_telephone] ,[IT_manager_mobile] ,[IT_manager_email] ,[EA_system] ,[PA_system] ,[Other_1] ,[Other_2] ,[PUL] ,[Note] ,[Inhabitant_level] FROM " + databasePrefix + "Customer WHERE Representative = @representive";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();
                command.Parameters.AddWithValue("@representive", representive);

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        view_Customer k = new view_Customer();
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            k.SetValue(k.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                        list.Add(k);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get the names of all customers
        /// </summary>
        /// <returns>A list of customer names</returns>
        public static List<String> getCustomerNames()
        {
            List<view_Customer> ProductReportRows = getAllCustomers();
            List<String> sortedNames = new List<String>();

            foreach (view_Customer c in ProductReportRows)
            {
                sortedNames.Add(c.Customer);
            }

            sortedNames.Sort();

            return sortedNames;
        }

        /// <summary>
        /// Get the names of all customers with a specific representative
        /// </summary>
        /// <param name="representative">The sign of the representative.</param>
        /// <returns>A list of customer names</returns>
        public static List<String> getCustomerNames(String representative)
        {
            List<view_Customer> ProductReportRows = getAllCustomers();
            List<String> sortedNames = new List<String>();

            foreach (view_Customer c in ProductReportRows)
            {
                if(c.Representative == representative)
                    sortedNames.Add(c.Customer);
            }

            sortedNames.Sort();

            return sortedNames;
        }
    }

}
