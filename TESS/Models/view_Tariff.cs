using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
   public class view_Tariff : SQLBaseClass
	{
		private float inhabitant_level;
		public float Inhabitant_level { get{ return inhabitant_level; } set{ inhabitant_level = value; } }

		private decimal price_category;
		public decimal Price_category { get{ return price_category; } set{ price_category = value; } }

		private decimal? license;
		public decimal? License { get{ return license; } set{ license = value; } }

		private decimal? maintenance;
		public decimal? Maintenance { get{ return maintenance; } set{ maintenance = value; } }

		private DateTime? valid_through;
		public DateTime? Valid_through { get{ return valid_through; } set{ valid_through = value; } }

		private long ssma_timestamp;
		public long SSMA_timestamp { get{ return ssma_timestamp; } set{ ssma_timestamp = value; } }

		public view_Tariff() : base("Tariff")
		{
			//ctr
		}

        /// <summary>
        /// Gets all tariff data.
        /// </summary>
        /// <returns>A list with all the tarrif rows.</returns>
        public static List<view_Tariff> getAllTariff()
        {

            List<view_Tariff> list = new List<view_Tariff>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [Inhabitant_level] ,[Price_category] ,[License] ,[Maintenance] ,[Valid_through], CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp FROM " + databasePrefix + "Tariff";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                            view_Tariff t = new view_Tariff();
                            //long l = reader.GetInt64(0);
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
            return list;
        }
    }

}