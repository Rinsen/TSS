using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TietoCRM.Models
{
    /// <summary>
    /// Rak vy mot tabell ModuleText
    /// </summary>
    public class view_ModuleText : SQLBaseClass
    {
        /// <summary>
        /// Räknare (identity) Primary Key
        /// </summary>
        public int _ID { get; set; }

        /// <summary>
        /// Offert (O) eller Avtal (A)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// OffertNr alt UnikId (enligt A_Avtalsregister)
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Artikel (A) alt Konsult (K)
        /// </summary>
        public string ModuleType { get; set; }

        /// <summary>
        /// V_Module.ArtikelNr alt konsult.Kod
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Modultexten
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public short Order { get; set; }

        /// <summary>
        /// Borttagen (1), default (0)
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Ändrat datum
        /// </summary>
        public DateTime Changed { get; set; }

        /// <summary>
        /// Ändrad av (sign)
        /// </summary>
        public string ChangedBy { get; set; }

        /// <summary>
        /// Skapad datum
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Skapad av (sign)
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Konstruktor
        /// </summary>
        public view_ModuleText() 
            : base("ModuleText")
        {
        }

        /// <summary>
        /// Hämta alla modultexter
        /// </summary>
        /// <param name="type">Typ av text, Offert eller Avtal</param>
        /// <param name="typeId">OffertNr alt UnikId (enligt A_Avtalsregister)</param>
        /// <returns></returns>
        public static List<view_ModuleText> getAllModuleTexts(string type, int typeId)
        {
            List<view_ModuleText> list = new List<view_ModuleText>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = " SELECT * FROM " + databasePrefix + "ModuleText";
                SqlCommand command = new SqlCommand(query, connection);
                if (string.IsNullOrEmpty(type) && typeId > 0)
                {
                    //query += " WHERE [Parent_article_number] = @parent_article_number ";
                    //command = new SqlCommand(query, connection);
                    //command.Parameters.Add("@parent_article_number", SqlDbType.Int).Value = parent_article_number;
                }

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_ModuleText k = new view_ModuleText();
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
    }
}