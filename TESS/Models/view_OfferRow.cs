using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TietoCRM.Extensions;
using System.Dynamic;

namespace TietoCRM.Models
{
    public class view_OfferRow : SQLBaseClass
    {
        private int offer_number;
        public int Offer_number { get { return offer_number; } set { offer_number = value; } }

        private int article_number;
        public int Article_number { get { return article_number; } set { article_number = value; } }

        private decimal? license;
        public decimal? License { get { return license; } set { license = value; } }

        private decimal? maintenance;
        public decimal? Maintenance { get { return maintenance; } set { maintenance = value; } }

        private bool? include_status;
        public bool? Include_status { get { return include_status; } set { include_status = value; } }

        private int fixed_price;
        public int Fixed_price { get; set; }

        private DateTime ssma_timestamp;
        public DateTime SSMA_timestamp { get { return ssma_timestamp; } set { ssma_timestamp = value; } }

        private String alias;
        public String Alias { get { return alias; } set { alias = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private bool includeDependencies;
        public bool IncludeDependencies { get { return includeDependencies; } set { includeDependencies = value; } }

        private static int ASort { get; set; }
        //private int ASort;
        private static string OrderBy { get; set; }

        /// <summary>
        /// Offer description for module, needed for new dialog "Edit Module Info". Not saved to database.
        /// </summary>
        public string _OfferDescription { get; set; }
        
        /// <summary>
        /// Id for ModuleText, not in DB 
        /// </summary>
        public int _ModuleTextId { get; set; }


        public view_OfferRow()
            : base("OfferRow")
        {
            //ctr
        }

        /// <summary>
        /// Compares offer rows on article number,
        /// </summary>
        /// <param name="obj">The other offer row</param>
        /// <returns>true/false depending on the compare result.</returns>
        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            view_OfferRow p = obj as view_OfferRow;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            if (String.IsNullOrEmpty(this.Alias))
                return (this.Article_number == p.Article_number);
            else
                return (this.Alias == p.Alias);
        }

        /// <summary>
        /// Gets all the offer rows with a specific offer number
        /// </summary>
        /// <param name="offerNumber">The offer number</param>
        /// <param name="area">The offer area</param>
        /// <returns>A list of offer rows with a specific offer number.</returns>
        public static List<view_OfferRow> getAllOfferRows(String offerNumber, String area)
        {
            List<view_OfferRow> list = new List<view_OfferRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = @"SELECT Offer_number, O.Article_number, License, 
                                        Maintenance, Include_status, O.Fixed_price, CAST(O.SSMA_timestamp AS BIGINT) AS SSMA_timestamp
                                        , Alias, O.Area, IncludeDependencies FROM " + databasePrefix + "OfferRow O " +
                                        "JOIN " + databasePrefix + "Module M ON M.Article_number = O.Article_number " +
                                        "WHERE Offer_number = @offerNumber AND O.Area = @area Order By " + GetOrderByForGetAllOfferRows();

                command.Prepare();
                command.Parameters.AddWithValue("@offerNumber", offerNumber);
                command.Parameters.AddWithValue("@area", area);


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_OfferRow t = new view_OfferRow();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }

                            //Also get Offer descritption..
                            view_Module module = new view_Module();
                            module.Select("Article_number = " + t.Article_number.ToString());
                            view_ModuleText moduleText = new view_ModuleText();
                            moduleText.Select("Type = 'O' AND TypeId = " + t.Offer_number.ToString() + " AND ModuleType = 'A' AND ModuleId = " + t.Article_number.ToString());

                            t._OfferDescription = moduleText.Description;
                            t._ModuleTextId = moduleText._ID;

                            list.Add(t);
                        }
                    }
                }


            }
            return list;
        }
        /// <summary>
        /// Gets all the offer rows
        /// </summary>
        /// <returns>A list of all offer rows.</returns>
        public static List<view_OfferRow> getAllOfferRows()
        {
            List<view_OfferRow> list = new List<view_OfferRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();


                // Default query
                command.CommandText = @"SELECT Offer_number, Article_number, License, 
                                        Maintenance, Include_status, Fixed_price, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp 
                                        ,Alias, Area, IncludeDependencies FROM " + databasePrefix + "OfferRow Order By " + GetOrderBy();

                command.Prepare();


                command.ExecuteNonQuery();


                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_OfferRow t = new view_OfferRow();
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

        /// <summary>
        /// Hämtar offertrader för att bygga ihop modultexter på offerten
        /// </summary>
        /// <param name="offerNumber"></param>
        /// <returns></returns>
        public List<dynamic> GetOfferRowsForModuleInfo(int offerNumber)
        {
            List<dynamic> list = new List<dynamic>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                // Default query
                //command.CommandText = @"SELECT Alias, Description FROM qry_OfferArtDescription Where Offertnr = @offerNumber Order By Typ, Alias";

                command.CommandText = @"SELECT Q.Alias, Q.Description, Q.Typ, Q.Art_id, M.System AS System FROM qry_OfferArtDescription Q 
                                        JOIN " + databasePrefix + @"Module M ON M.Article_number = Q.Art_id
                                        JOIN " + databasePrefix + @"Sector S ON S.System = M.System and S.Classification = M.Classification
                                        WHERE OFFERTNR = @offerNumber 
                                        ORDER BY " + GetOrderByForQry();

                command.Prepare();
                command.Parameters.AddWithValue("@offerNumber", offerNumber);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            dynamic t = new ExpandoObject();
                            t.Alias = reader.GetValue(0);
                            t.Offer_description = reader.GetValue(1);
                            list.Add(t);
                        }
                    }
                }
            }

            return list;
        }

        private string GetOrderByForQry()
        {
            ASort = HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "Typ, System, Alias";
            if (ASort == 2) return "Typ, M.Classification, Alias";
            if (ASort == 3) return "Typ, System, Article_number";
            if (ASort == 4) return "Typ, S.SortNo, M.Classification, ISNULL(NULLIF(Sort_order, 0), 99), Alias";
            return "Alias";
        }

        private static string GetOrderByForGetAllOfferRows()
        {
            ASort = HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "Alias";
            if (ASort == 2) return "M.Classification, Alias";
            if (ASort == 3) return "M.Classification, Article_number";
            return "Alias";
        }

        private static string GetOrderBy()
        {
            ASort = HttpContext.Current.GetUser().AvtalSortera;
            if (ASort == 1) return "Alias";
            if (ASort == 2) return "Classification, Alias";
            if (ASort == 3) return "Classification, Article_number";
            return "Alias";
        }

        /// <summary>
        /// Gets all offer rows for specific offer id
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns></returns>
        public static List<view_OfferRow> getOfferRows(int offerId)
        {
            List<view_OfferRow> list = new List<view_OfferRow>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                // Default query
                command.CommandText = @"SELECT Offer_number, Article_number, License, 
                                        Maintenance, Include_status, Fixed_price, CAST(SSMA_timestamp AS BIGINT) AS SSMA_timestamp 
                                        ,alias, Area, IncludeDependencies FROM " + databasePrefix + "OfferRow where Offer_number = @offerNumber Order By " + GetOrderBy();

                command.Prepare();
                command.Parameters.AddWithValue("@offerNumber", offerId);

                command.ExecuteNonQuery();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            view_OfferRow t = new view_OfferRow();
                            int i = 0;
                            while (reader.FieldCount > i)
                            {
                                t.SetValue(t.GetType().GetProperties()[i].Name, reader.GetValue(i));
                                i++;
                            }

                            //Also get Offer descritption..
                            view_Module module = new view_Module();
                            module.Select("Article_number = " + t.Article_number.ToString());
                            view_ModuleText moduleText = new view_ModuleText();
                            moduleText.Select("Type = 'O' AND TypeId = " + t.Offer_number.ToString() + " AND ModuleType = 'A' AND ModuleId = " + t.Article_number.ToString());

                            t._OfferDescription = moduleText.Description;
                            t._ModuleTextId = moduleText._ID;

                            list.Add(t);
                        }
                    }
                }
            }
            return list;
        }
    }

}