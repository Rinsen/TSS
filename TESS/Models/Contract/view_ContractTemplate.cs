using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ContractTemplate : SQLBaseClass
    {
        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String title1;
        public String Title1 { get { return title1; } set { title1 = value; } }

        private String title2;
        public String Title2 { get { return title2; } set { title2 = value; } }

        private String title3;
        public String Title3 { get { return title3; } set { title3 = value; } }

        private String title4;
        public String Title4 { get { return title4; } set { title4 = value; } }

        private String title5;
        public String Title5 { get { return title5; } set { title5 = value; } }

        private String title6;
        public String Title6 { get { return title6; } set { title6 = value; } }

        private String title7;
        public String Title7 { get { return title7; } set { title7 = value; } }

        private String title8;
        public String Title8 { get { return title8; } set { title8 = value; } }

        private String title9;
        public String Title9 { get { return title9; } set { title9 = value; } }

        private String title10;
        public String Title10 { get { return title10; } set { title10 = value; } }

        private String title11;
        public String Title11 { get { return title11; } set { title11 = value; } }

        private String title12;
        public String Title12 { get { return title12; } set { title12 = value; } }

        private String title13;
        public String Title13 { get { return title13; } set { title13 = value; } }

        private String title14;
        public String Title14 { get { return title14; } set { title14 = value; } }

        private String title15;
        public String Title15 { get { return title15; } set { title15 = value; } }

        private String title16;
        public String Title16 { get { return title16; } set { title16 = value; } }

        private String title17;
        public String Title17 { get { return title17; } set { title17 = value; } }

        private String title18;
        public String Title18 { get { return title18; } set { title18 = value; } }

        private String title19;
        public String Title19 { get { return title19; } set { title19 = value; } }

        private String title20;
        public String Title20 { get { return title20; } set { title20 = value; } }

        private String title21;
        public String Title21 { get { return title21; } set { title21 = value; } }

        private String title22;
        public String Title22 { get { return title22; } set { title22 = value; } }

        private String title23;
        public String Title23 { get { return title23; } set { title23 = value; } }

        private String title24;
        public String Title24 { get { return title24; } set { title24 = value; } }

        private String title25;
        public String Title25 { get { return title25; } set { title25 = value; } }

        private String title26;
        public String Title26 { get { return title26; } set { title26 = value; } }

        private String title27;
        public String Title27 { get { return title27; } set { title27 = value; } }

        private String title28;
        public String Title28 { get { return title28; } set { title28 = value; } }

        private String text1;
        public String Text1 { get { return text1; } set { text1 = value; } }

        private String text2;
        public String Text2 { get { return text2; } set { text2 = value; } }

        private String text3;
        public String Text3 { get { return text3; } set { text3 = value; } }

        private String text4;
        public String Text4 { get { return text4; } set { text4 = value; } }

        private String text5;
        public String Text5 { get { return text5; } set { text5 = value; } }

        private String text6;
        public String Text6 { get { return text6; } set { text6 = value; } }

        private String text7;
        public String Text7 { get { return text7; } set { text7 = value; } }

        private String text8;
        public String Text8 { get { return text8; } set { text8 = value; } }

        private String text9;
        public String Text9 { get { return text9; } set { text9 = value; } }

        private String text10;
        public String Text10 { get { return text10; } set { text10 = value; } }

        private String text11;
        public String Text11 { get { return text11; } set { text11 = value; } }

        private String text12;
        public String Text12 { get { return text12; } set { text12 = value; } }

        private String text13;
        public String Text13 { get { return text13; } set { text13 = value; } }

        private String text14;
        public String Text14 { get { return text14; } set { text14 = value; } }

        private String text15;
        public String Text15 { get { return text15; } set { text15 = value; } }

        private String text16;
        public String Text16 { get { return text16; } set { text16 = value; } }

        private String text17;
        public String Text17 { get { return text17; } set { text17 = value; } }

        private String text18;
        public String Text18 { get { return text18; } set { text18 = value; } }

        private String text19;
        public String Text19 { get { return text19; } set { text19 = value; } }

        private String text20;
        public String Text20 { get { return text20; } set { text20 = value; } }

        private String text21;
        public String Text21 { get { return text21; } set { text21 = value; } }

        private String text22;
        public String Text22 { get { return text22; } set { text22 = value; } }

        private String text23;
        public String Text23 { get { return text23; } set { text23 = value; } }

        private String prolog;
        public String Prolog { get { return prolog; } set { prolog = value; } }

        private String epilog;
        public String Epilog { get { return epilog; } set { epilog = value; } }

        private String moduleText;
        public String ModuleText { get { return moduleText; } set { moduleText = value; } }

        private String contract_Description;
        public String Contract_Description { get { return contract_Description; } set { contract_Description = value; } }

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_ContractTemplate()
            : base("ContractTemplate")
        {
            //ctr
        }

        /// <summary>
        /// Gets the main contract templates of a customer.
        /// </summary>
        /// <param name="customer">The customer</param>
        /// <returns>A list of contract templates.</returns>
        public static List<view_ContractTemplate> GetAllContractTemplates(String customer)
        {
            List<view_ContractTemplate> list = new List<view_ContractTemplate>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT [Contract_id] ,[Customer] ,[Title1] ,[Title2] ,[Title3] ,[Title4] ,[Title5] ,[Title6] ,[Title7] ,[Title8] ,[Title9] ,[Title10] ,[Title11] ,[Title12] ,[Title13] ,[Title14] ,[Title15] ,[Title16] ,[Title17] ,[Title18] ,[Title19] ,[Title20] ,[Title21] ,[Title22] ,[Title23] ,[Title24] ,[Title25] ,[Title26] ,[Title27] ,[Title28] ,[text1] ,[text2] ,[text3] ,[text4] ,[text5] ,[text6] ,[text7] ,[text8] ,[text9] ,[text10] ,[text11] ,[text12] ,[text13] ,[text14] ,[text15] ,[text16] ,[text17] ,[text18] ,[text19] ,[text20] ,[text21] ,[text22] ,[text23] ,[Prolog] ,[Epilog], Contract_Description CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "ContractTemplate WHERE " + "Customer = @customer";

                command.Prepare();
                command.Parameters.AddWithValue("@customer", customer);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractTemplate t = new view_ContractTemplate();
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


    }

}