using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ContractText : SQLBaseClass
    {
        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String contract_type;
        public String Contract_type { get { return contract_type; } set { contract_type = value; } }

        private String document_head;
        public String Document_head { get { return document_head; } set { document_head = value; } }

        private String page_head;
        public String Page_head { get { return page_head; } set { page_head = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private String delivery_maint_title;
        public String Delivery_maint_title { get { return delivery_maint_title; } set { delivery_maint_title = value; } }

        private String delivery_maint_text;
        public String Delivery_maint_text { get { return delivery_maint_text; } set { delivery_maint_text = value; } }

        private String page_foot;
        public String Page_foot { get { return page_foot; } set { page_foot = value; } }

        private String document_foot_title;
        public String Document_foot_title { get { return document_foot_title; } set { document_foot_title = value; } }

        private String document_foot;
        public String Document_foot { get { return document_foot; } set { document_foot = value; } }

        private String module_info;
        public String Module_info { get { return module_info; } set { module_info = value; } }

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_ContractText()
            : base("ContractText")
        {
            //ctr
        }

        /// <summary>
        /// Gets the text templates of the additional contracts of a specific customer
        /// </summary>
        /// <param name="customer">The customers name</param>
        /// <returns>A list of contract texts.</returns>
        public static List<view_ContractText> GetAllContractTexts(String customer)
        {
            List<view_ContractText> list = new List<view_ContractText>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [Contract_id] ,[Customer] ,[Contract_type] ,[Document_head] ,[Page_head] ,[Title] ,";
                command.CommandText += "Delivery_maint_title, Delivery_maint_text, Page_foot, Document_foot_title, [Document_foot], Module_info,";
                command.CommandText += "CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ContractText WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractText t = new view_ContractText();
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
        public void UpdateModuleInfo(string customer, string contract_id, string moduleInfo, string contract_type)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                if (contract_type == "Huvudavtal")
                {
                    command.CommandText = "Update dbo.A_H_Text Set Modultext2 = @moduleInfo ";
                    command.CommandText += "Where avtalsid = @contract_id And Kund = @customer";
                }
                else
                {
                    // Default query
                    command.CommandText = "Update dbo.A_Text Set Modultext = @moduleInfo ";
                    command.CommandText += "Where avtalsid = @contract_id And Kund = @customer";
                }

                command.Prepare();
                command.Parameters.AddWithValue("@moduleInfo", moduleInfo);
                command.Parameters.AddWithValue("@contract_id", contract_id);
                command.Parameters.AddWithValue("@customer", customer);

                command.ExecuteNonQuery();
            }
        }

    }

}