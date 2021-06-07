using System;
using System.Collections.Generic;
using TietoCRM.Extensions;

namespace TietoCRM.Models
{
    public class view_AuditLog : SQLBaseClass
    {
        public string Type { get; set; }
        public string TableName { get; set; }
        public string PK { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UserName { get; set; }

        public view_AuditLog()
            : base("AuditLog")
        {
            //ctr
        }

        /// <summary>
        /// Write to AuditLog
        /// </summary>
        /// <param name="type">CRUD type</param>
        /// <param name="tableName">Changed table</param>
        /// <param name="pk">Primary key of changed table row</param>
        /// <param name="oldValue">Replace value</param>
        /// <param name="newValue">New value</param>
        public void Write(string type, string tableName, string pk, string newValue = "", string oldValue = "", string fieldName = "")
        {
            try
            {
                Type = type;
                TableName = tableName;
                PK = pk;
                FieldName = fieldName;
                OldValue = oldValue;
                NewValue = newValue;
                UpdateDate = DateTime.Now;
                UserName = System.Web.HttpContext.Current.GetUser().Sign;

                Insert();
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        internal void LogUserChanges(view_User user, Dictionary<string, object> variables)
        {
            object value = null;
            variables.TryGetValue("Sign", out value);
            if(user.Sign != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Sign, "Sign");
            }

            variables.TryGetValue("Name", out value);
            if (user.Name != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Name, "Name");
            }

            variables.TryGetValue("Area", out value);
            if (user.Area != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Area, "Area");
            }

            variables.TryGetValue("Offer_file_location", out value);
            if (user.Offer_file_location != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Offer_file_location, "Offer_file_location");
            }

            variables.TryGetValue("Contract_file_location", out value);
            if (user.Contract_file_location != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Contract_file_location, "Contract_file_location");
            }

            variables.TryGetValue("File_format", out value);
            if (user.File_format != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.File_format, "File_format");
            }

            variables.TryGetValue("Address", out value);
            if (user.Address != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Address, "Address");
            }

            variables.TryGetValue("City", out value);
            if (user.City != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.City, "City");
            }

            variables.TryGetValue("Telephone", out value);
            if (user.Telephone != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Telephone, "Telephone");
            }

            variables.TryGetValue("Mobile", out value);
            if (user.Mobile != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Mobile, "Mobile");
            }

            variables.TryGetValue("Windows_user", out value);
            if (user.Windows_user != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Windows_user, "Windows_user");
            }

            variables.TryGetValue("User_level", out value);
            decimal decimalValue = 0;
            if(decimal.TryParse(value.ToString(), out decimalValue))
            {
                if (user.User_level != decimalValue)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.User_level.ToString(), "User_level");
                }
            }

            variables.TryGetValue("Use_logo", out value);
            bool useLogo = false;
            if(bool.TryParse(value.ToString(), out useLogo))
            {
                if (user.Use_logo != useLogo)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.Use_logo.ToString(), "Use_logo");
                }
            }

            variables.TryGetValue("Std_sum_offert", out value);
            int stdSumOffert = 0;
            if(int.TryParse(value.ToString(), out stdSumOffert))
            {
                if (user.Std_sum_offert != stdSumOffert)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.Std_sum_offert.ToString(), "Std_sum_offert");
                }
            }

            variables.TryGetValue("Std_sum_kontrakt", out value);
            int stdSumKontrakt = 0;
            if(int.TryParse(value.ToString(), out stdSumKontrakt))
            {
                if (user.Std_sum_kontrakt != stdSumKontrakt)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.Std_sum_kontrakt.ToString(), "Std_sum_kontrakt");
                }
            }

            variables.TryGetValue("Kr_every_row", out value);
            int krEveryRow = 0;
            if(int.TryParse(value.ToString(), out krEveryRow))
            {
                if (user.Kr_every_row != krEveryRow)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.Kr_every_row.ToString(), "Kr_every_row");
                }
            }

            variables.TryGetValue("Reminder_Prompt", out value);
            int reminderPrompt = 0;
            if(int.TryParse(value.ToString(), out reminderPrompt))
            {
                if (user.Reminder_Prompt != reminderPrompt)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.Reminder_Prompt.ToString(), "Reminder_Prompt");
                }
            }

            variables.TryGetValue("Email", out value);
            if (user.Email != value.ToString())
            {
                Write("U", "view_User", user.Sign, (string)value, user.Email.ToString(), "Email");
            }

            variables.TryGetValue("AvtalSortera", out value);
            int avtalSortera = 0;
            if(int.TryParse(value.ToString(), out avtalSortera))
            {
                if (user.AvtalSortera != avtalSortera)
                {
                    Write("U", "view_User", user.Sign, (string)value, user.AvtalSortera.ToString(), "AvtalSortera");
                }
            }
        }
    }
}