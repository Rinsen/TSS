using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models.Contract
{
    public class view_ContractExpirationList : SQLBaseClass
    {
        private bool extend;
        public bool Extend { get; set; }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String contract_type;
        public String Contract_type { get { return contract_type; } set { contract_type = value; } }

        private String title;
        public String Title { get { return title; } set { title = value; } }

        private DateTime? observation;
        public DateTime? Observation { get { return observation; } set { observation = value; } }

        private DateTime? valid_through;
        public DateTime? Valid_through { get { return valid_through; } set { valid_through = value; } }

        private String status;
        public String Status { get { return status; } set { status = value; } }

        private double? extension;
        public double? Extension { get { return extension; } set { extension = value ?? 0; } }

        private short? term_of_notice;
        public short? Term_of_notice { get { return term_of_notice; } set { term_of_notice = value ?? 0; } }

        private DateTime? expire;
        public DateTime? Expire { get { return expire; } set { expire = value; } }

        private String sign;
        public String Sign { get { return sign; } set { sign = value; } }

        private int canExtend;
        public int CanExtend { get; set; }

        private String representative;
        public String Representative { get { return representative; } set { representative = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }


        public view_ContractExpirationList()
            : base("ContractExpirationList")
        {
            //ctr
        }

        public static List<view_ContractExpirationList> GetContractExpirationList(String representative)
        {
            List<view_ContractExpirationList> list = new List<view_ContractExpirationList>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = "SELECT * ";
                command.CommandText += "FROM qry_ContractExpirationList WHERE " + "Contract_type = @contract_type And [status] = @status And Representative = @sign Order By Observation";

                command.Prepare();
                command.Parameters.AddWithValue("@contract_type", "Huvudavtal");
                command.Parameters.AddWithValue("@status", "Giltigt");
                command.Parameters.AddWithValue("@sign", representative);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 1;
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_ContractExpirationList t = new view_ContractExpirationList();
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

        public static int ExtendContract(string avtalsid, string kund ){
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = "Update dbo.A_avtalsregister Set giltigttom = DateAdd(m,förläng,giltigttom), bevakning = DateAdd(m,0-uppsägningstid,DateAdd(m,förläng,giltigttom)), ExpirationList = 1 ";
                command.CommandText += "Where avtalsid = @avtalsid And Kund = @kund";

                command.Prepare();
                command.Parameters.AddWithValue("@avtalsid", avtalsid);
                command.Parameters.AddWithValue("@kund", kund);

                command.ExecuteNonQuery();

                return 1;
            }
        }
    }
}