using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TietoCRM.Models
{
    /// <summary>
    /// Rak vy mot tabell ModuleText
    /// </summary>
    public class view_OrganisationInformation : SQLBaseClass
    {
        /// <summary>
        /// Räknare (identity) Primary Key
        /// </summary>
        public int _ID { get; set; }

        /// <summary>
        /// Organisationsnummer
        /// </summary>
        public string OrgNo { get; set; }

        /// <summary>
        /// Företagsnamn
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Standardvärde i vy-dialog
        /// </summary>
        public bool IsDefaultValue { get; set; }

        /// <summary>
        /// Skapad datum
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Skapad av handläggare
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Ändrad datum
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Ändrad av handläggare
        /// </summary>
        public string ModifiedBy { get; set; }


        /// <summary>
        /// Konstruktor
        /// </summary>
        public view_OrganisationInformation() 
            : base("OrganisationInformation")
        {
        }

        /// <summary>
        /// Hämta alla organisationsnummer
        /// </summary>
        /// <returns></returns>
        public static List<view_OrganisationInformation> getAllOrganisations()
        {
            List<view_OrganisationInformation> list = new List<view_OrganisationInformation>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = " SELECT * FROM " + databasePrefix + "OrganisationInformation";
                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_OrganisationInformation k = new view_OrganisationInformation();
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

        public static void UpdateOrganisationInformationDefaultValue(int orgInfoId, bool isDefaultOrganisation)
        {
            view_OrganisationInformation orgInfo = new view_OrganisationInformation();
            
            if(isDefaultOrganisation)
            {
                //Reset default values (only one)
                orgInfo.Select("IsDefaultValue = 1");
                if(orgInfo._ID != 0)
                {
                    orgInfo.IsDefaultValue = false;
                    orgInfo.Update("ID = " + orgInfo._ID);
                }
            }

            orgInfo.Select("ID = " + orgInfoId);
            
            orgInfo.IsDefaultValue = isDefaultOrganisation;
            
            orgInfo.Update("ID = " + orgInfoId);

        }
    }
}