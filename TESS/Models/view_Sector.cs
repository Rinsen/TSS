using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace TietoCRM.Models
{
    public class view_Sector : SQLBaseClass
    {
        private decimal id_pk;
        public decimal _ID_PK { get { return id_pk; } set { id_pk = value; } }

        private String system;
        public String System { get { return system; } set { system = value; } }

        private String classification;
        public String Classification { get { return classification; } set { classification = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private decimal sortno;
        public decimal SortNo { get { return sortno; } set { sortno = value; } }

        private String shortname;
        public String ShortName { get { return shortname; } set { shortname = value; } }

        private String price_type;
        public String Price_type { get { return price_type; } set { price_type = value; } }

        public view_Sector()
            : base("Sector")
        {
            //ctr
        }

        public override int Insert()
        {
            this.InsertSelectOptions();
            return base.Insert();
        }

        public override void Update(string condition)
        {
            this.InsertSelectOptions();
            base.Update(condition);
        }

        private void InsertSelectOptions()
        {
            SelectOptions<view_Sector> selectOptions = new SelectOptions<view_Sector>();
            view_SelectOption so = new view_SelectOption();
            so.Model = this.GetType().Name;
            if (!selectOptions.Options["System"].Any(d => d.Value == this.System))
            {
                so.Property = "System";
                so.Text = this.System;
                so.Value = this.System;
                so.Insert();
            }
            if (!selectOptions.Options["Classification"].Any(d => d.Value == this.Classification))
            {
                so.Property = "Classification";
                so.Text = this.Classification;
                so.Value = this.Classification;
                so.Insert();
            }
            if (!selectOptions.Options["Area"].Any(d => d.Value == this.Area))
            {
                so.Property = "Area";
                so.Text = this.Area;
                so.Value = this.Area;
                so.Insert();
            }
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Sector> getAllSectors()
        {
            List<view_Sector> list = new List<view_Sector>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID_PK], [System], [Classification], [Area], [SortNo], [ShortName], [Price_type] FROM " + databasePrefix + "Sector";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Sector k = new view_Sector();
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

        public static HashSet<String> getAllAreas()
        {
            SelectOptions<view_Sector> selectOptions = new SelectOptions<view_Sector>();
            HashSet<String> hs = new HashSet<string>();

            foreach(SelectOptions<view_Sector>.SelectOption so in selectOptions.GetOptions("Area"))
            {
                hs.Add(so.Text);
            }

            return hs;
        }
    }

}