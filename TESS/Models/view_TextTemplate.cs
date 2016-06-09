using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_TextTemplate : SQLBaseClass
    {
        private int id_pk;
        public int ID_PK { get { return id_pk; } set { id_pk = value; } }

        private String document_type;
        public String Document_type { get { return document_type; } set { document_type = value; } }

        private String short_descr;
        public String Short_descr { get { return short_descr; } set { short_descr = value; } }

        private String sign;
        public String Sign { get { return sign; } set { sign = value; } }

        private int template_number;
        public int Template_number { get { return template_number; } set { template_number = value; } }

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

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        public view_TextTemplate()
            : base("TextTemplate")
        {
            //ctr
        }

        /// <summary>
        /// Gets all text templates of a specific user
        /// </summary>
        /// <param name="sign">null if all users, Otherwise the sign of the user.</param>
        /// <returns>A list of all templates.</returns>
        public static List<view_TextTemplate> getAllTextTemplates(String sign)
        {
            List<view_TextTemplate> list = new List<view_TextTemplate>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = "SELECT ID_PK, Document_type, Short_descr, Sign, Template_number, Document_head, Page_head, Title, ";
                command.CommandText += "Delivery_maint_title, Delivery_maint_text, Page_foot, Document_foot_title, Document_foot, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp ";
                command.CommandText += "FROM " + databasePrefix + "TextTemplate";

                // Default querry
                if (sign != null)
                {
                    command.CommandText += " WHERE Sign = @sign";
                    command.Prepare();
                    command.Parameters.AddWithValue("@sign", sign);
                }

                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_TextTemplate t = new view_TextTemplate();
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


        public static List<view_TextTemplate> getTextTemplatesType(String document_type, String sign)
        {
            List<view_TextTemplate> list = new List<view_TextTemplate>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default querry
                command.CommandText = "SELECT ID_PK, Document_type, Short_descr, Sign, Template_number, Document_head, Page_head, Title, Delivery_maint_title, Delivery_maint_text, ";
                command.CommandText += "Page_foot, Document_foot_title, Document_foot, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM ";
                command.CommandText += databasePrefix + "TextTemplate WHERE Document_type Like @document_type and (Sign = @sign Or Sign = 'Alla')";
                
                if (document_type == "Avtal")
                {
                    command.Prepare();
                    command.Parameters.AddWithValue("@document_type", "%av%");
                    command.Parameters.AddWithValue("@sign", sign);
                }
                else
                {
                    command.Prepare();
                    command.Parameters.AddWithValue("@document_type", "%Offert%");
                    command.Parameters.AddWithValue("@sign", sign);
                }

                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_TextTemplate t = new view_TextTemplate();
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
            list.OrderBy(m => m.Sign).ThenBy(m => m.Short_descr);
            return list;
        }

    }
}