using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.ServiceModel.Description;
using TietoCRM.Extensions;

namespace TietoCRM.Models
{
    /// <summary>
    /// Rak vy mot tabell SaaS_Formula
    /// </summary>
    public class view_SaaS_Formula : SQLBaseClass
    {
        /// <summary>
        /// Räknare (identity) Primary Key
        /// </summary>
        public int _ID { get; set; }

        /// <summary>
        /// LicensePart
        /// </summary>
        public decimal? LicensePart { get; set; }

        /// <summary>
        /// Factor
        /// </summary>
        public decimal? Factor { get; set; }

        /// <summary>
        /// PeriodFrom
        /// </summary>
        public DateTime? PeriodFrom { get; set; }

        /// <summary>
        /// PeriodTo
        /// </summary>
        public DateTime? PeriodTo { get; set; }

        /// <summary>
        /// Activate SaaS formula function
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Skapad datum
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Skapad av (sign)
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Ändrat datum
        /// </summary>
        public DateTime? Changed { get; set; }

        /// <summary>
        /// Ändrad av (sign)
        /// </summary>
        public string ChangedBy { get; set; }


        /// <summary>
        /// Konstruktor
        /// </summary>
        public view_SaaS_Formula() 
            : base("SaaS_Formula")
        {
        }

        /// <summary>
        /// Hämta aktuell SaaS formel
        /// </summary>
        /// <returns></returns>
        public static view_SaaS_Formula getActiveSaaSFormula()
        {
            view_SaaS_Formula currentActiveFormula = new view_SaaS_Formula();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = " SELECT * FROM " + databasePrefix + "SaaS_Formula WHERE PeriodTo IS NULL";
                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int i = 0;
                        while (reader.FieldCount > i)
                        {
                            currentActiveFormula.SetValue(currentActiveFormula.GetType().GetProperties()[i].Name, reader.GetValue(i));
                            i++;
                        }
                    }
                }
            }

            return currentActiveFormula;
        }

        /// <summary>
        /// Save new SaaS-formula period
        /// </summary>
        /// <param name="activate"></param>
        /// <param name="licensePart"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static view_SaaS_Formula SaveSaaSFormula(bool isActive, decimal licensePart, decimal factor)
        {
            try
            {
                var oldFormula = getActiveSaaSFormula();

                if(oldFormula != null && oldFormula._ID > 0)
                {
                    //End previous period (history)
                    oldFormula.PeriodTo = DateTime.Now;
                    oldFormula.Changed = DateTime.Now;
                    oldFormula.ChangedBy = System.Web.HttpContext.Current.GetUser().Sign;
                    oldFormula.Update("Id=" + oldFormula._ID);
                }

                view_SaaS_Formula saasFormula = new view_SaaS_Formula
                {
                    IsActive = isActive,
                    LicensePart = licensePart,
                    Factor = factor,

                    PeriodFrom = DateTime.Now,
                    PeriodTo = null,

                    CreatedBy = System.Web.HttpContext.Current.GetUser().Sign,
                    Created = DateTime.Now,
                };

                saasFormula.Insert();
                return saasFormula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        /// <summary>
        /// Calculates SaaS sum with SaaS formula
        /// (License / {LicensePart} + Maintenance) * {Factor}
        /// </summary>
        /// <param name="license"></param>
        /// <param name="maintenance"></param>
        /// <param name="licensePart"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Decimal CalculateSaasPrice(decimal? license, decimal? maintenance, decimal? licensePart, decimal? factor)
        {
            try
            {
                decimal result = maintenance.Value;
                if(factor > 0 && licensePart > 0 && license.HasValue && maintenance.HasValue)
                {
                    result = ((license.Value / licensePart.Value) + maintenance.Value) * factor.Value;
                }

                return Math.Round(result, 0);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}