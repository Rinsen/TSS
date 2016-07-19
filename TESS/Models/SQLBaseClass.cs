using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Web.Security.AntiXss;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace TietoCRM.Models
{
    /// <summary>
    /// A base class make it easy to SELECT, UPDATE, DELETE and INSERT values to the database. If you have a table in database you can make a class
    /// that has same property names as it has row names, then this class will handle all things you want to related to SELECT, UPDATE, DELETE and INSERT
    /// </summary>
    public abstract class SQLBaseClass
    {
        protected static String databasePrefix = "dbo.view_";
        private String table;
        protected static String connectionString = ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString;
       
        private PropertyInfo[] propertyInfos;
        private String joinSQL;

        protected String JoinSQL
        {
            get { return joinSQL; }
            set { joinSQL = value; }
        }
        /// <summary>
        /// Appends a join string to the joinSQL variable
        /// </summary>
        /// <param name="value">
        /// the String you want to append onto the variable
        /// </param>
        public void AppendJoinSQL(String value)
        {
            joinSQL += value;
        }

        protected PropertyInfo[] PropertyInfos
        {
            get { return propertyInfos; }
            private set { propertyInfos = value; }
        }
        /// <summary>
        /// Choose a SQL table you want this class to work with.
        /// </summary>
        public SQLBaseClass(String table)
        {
            this.table = table;
            this.propertyInfos = this.GetType().GetProperties();
        }

        /// <summary>
        ///     This method will convert the value to correct type and put in the variable that the class has
        /// </summary>
        /// <param name="variableName">The variable name in the class you want to change</param>
        /// <param name="value">The value you want to change to</param>
        public void SetValue(String variableName, Object value)
        {
            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                if (pi.Name == variableName)
                {
                    if (pi.PropertyType == typeof(short) || pi.PropertyType == typeof(short?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToInt16(0));
                        else
                            pi.SetValue(this, Convert.ToInt16(value));
                        break;
                    }
                    else if (pi.PropertyType == typeof(bool) || pi.PropertyType == typeof(bool?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToBoolean(false));
                        else if (value.GetType() == typeof(String))
                        {
                            if(value.ToString() == "1")
                                pi.SetValue(this, true);
                            else if(value.ToString() == "0")
                                pi.SetValue(this, false);
                            else
                                pi.SetValue(this, Boolean.Parse(value.ToString()));
                        }
                        else
                            pi.SetValue(this, Convert.ToBoolean(value));
                        break;
                    }
                    else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(int?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToInt32(0));
                        else
                            pi.SetValue(this, Convert.ToInt32(value));
                        break;
                    }
                    else if (pi.PropertyType == typeof(float) || pi.PropertyType == typeof(float?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToSingle(0));
                        else
                            pi.SetValue(this, Convert.ToSingle(value));
                        break;
                    }
                    else if (pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(decimal?))
                    {
                        if(value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToDecimal(0));
                        else
                            pi.SetValue(this, Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                        break;
                    }
                    else if (pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
                    {
                        if (value == null)
                        {
                            pi.SetValue(this, null);
                            break;
                        }
                        else
                        {
                            if (value.GetType() == typeof(System.Int64))
                            {
                                DateTime dt = new DateTime();
                                dt.AddMilliseconds((long)value);
                                pi.SetValue(this, dt);
                            }
                            else if (value.GetType() == typeof(DateTime))
                                pi.SetValue(this, value);
                            else if (value.GetType() == typeof(System.DBNull))
                                pi.SetValue(this, null);
                            else
                                pi.SetValue(this, DateTime.Parse((String)value));
                            break;
                        }
                    }
                    else if (pi.PropertyType == typeof(System.Int64) || pi.PropertyType == typeof(System.Int64?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToInt64(0));
                        else
                            pi.SetValue(this, Convert.ToInt64(value));
                        break;
                    }
                    else if (pi.PropertyType == typeof(double) || pi.PropertyType == typeof(double?))
                    {
                        if (value == DBNull.Value || value == null)
                            pi.SetValue(this, Convert.ToDouble(0));
                        else
                            pi.SetValue(this, Convert.ToDouble(value));
                        break;
                    }
                    else if (value == null || value.GetType() == typeof(System.DBNull))
                    {
                        pi.SetValue(this, null);
                        break;
                    }
                    else
                    {
                        if (value.GetType() == typeof(System.DBNull))
                            pi.SetValue(this, null);
                        else if (value.GetType() == typeof(float))
                        {
                            if(pi.PropertyType == typeof(double?))
                                pi.SetValue(this, Convert.ToDouble(value));
                            else
                                pi.SetValue(this, Convert.ToSingle(value));
                        }
                        else
                            pi.SetValue(this, value);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// send the objects variables to the database.
        /// </summary>
        /// <param name="condition">
        /// is what condition you want to use for the SQL query to know what row to affect. For example "ID = 1".
        /// </param>
        public virtual void Update(string condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "UPDATE " + databasePrefix + this.table + " SET ";

                foreach (PropertyInfo pi in this.propertyInfos)
	            {
                    if (pi.Name.ToLower() != "ssma_timestamp" && !pi.Name.StartsWith("_") && !pi.Name.Equals("ID_PK"))
                    {
                        query += "" + pi.Name + "=";
                        query += "@" + pi.Name + ",";
                    }
	            }
                query = query.Remove(query.Length - 1);
                SqlCommand command = connection.CreateCommand();
                query += " WHERE " + ParameterizeCondition(condition, ref command);

                command.CommandText = query;

                foreach(PropertyInfo pi in this.PropertyInfos)
                {
                    if (pi.Name.ToLower() != "ssma_timestamp" && !pi.Name.StartsWith("_") && !pi.Name.Equals("ID_PK"))
                    {
                        Type t = pi.GetType();

                        SqlDbType sdt = SqlDbType.NVarChar;
                        if (pi.PropertyType == typeof(DateTime?) || pi.PropertyType == typeof(DateTime))
                            sdt = SqlDbType.DateTime;
                        else if (pi.PropertyType == typeof(Decimal?))
                            sdt = SqlDbType.Money;
                        else if (pi.PropertyType == typeof(bool))
                            sdt = SqlDbType.Bit;

                        SqlParameter sp = new SqlParameter("@" + pi.Name, sdt, -1);

                        if (pi.GetType() == typeof(System.String))
                            sp.Value = AntiXssEncoder.HtmlEncode((String)pi.GetValue(this), false);
                        else
                            sp.Value = pi.GetValue(this) ?? DBNull.Value;

                        command.Parameters.Add(sp);
                        //command.Parameters.AddWithValue("@" + pi.Name, pi.GetValue(this));
                    }
                }


                command.Prepare();
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }
        /// <summary>
        /// Updates the current object with its variables and send it to the database.
        /// </summary>
        /// <param name="condition">
        /// Is what condition you want to use for the SQL query to know what row to affect. For example "ID = 1".
        /// </param>
        /// <param name="value">
        /// The new value you want to set
        /// </param>
        /// <param name="type">
        /// The variable you want to change
        /// </param>
        /// <returns>
        /// A Number that represents what went wrong cr if it succeded it returns "1".
        /// </returns>
        public String Update(string condition, String value, String type)
        {
            try
            {
                foreach (PropertyInfo pi in this.GetType().GetProperties())
                {
                    if (pi.Name == type)
                    {
                        if (pi.PropertyType == typeof(short))
                        {
                            try
                            {
                                pi.SetValue(this, Convert.ToInt16(value));
                            }
                            catch (Exception e)
                            {
                                return "0";
                            }
                        }
                        else if (pi.PropertyType == typeof(bool))
                        {
                            try
                            {
                                pi.SetValue(this, Convert.ToBoolean(value));
                            }
                            catch (Exception e)
                            {
                                return "0";
                            }
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            try
                            {
                                pi.SetValue(this, Convert.ToInt32(value));
                            }
                            catch (Exception e)
                            {
                                return "0";
                            }
                        }
                        else if (pi.PropertyType == typeof(float))
                        {
                            try
                            {
                                pi.SetValue(this, Convert.ToSingle(value));
                            }
                            catch (Exception e)
                            {
                                return "0";
                            }
                        }
                        else
                            pi.SetValue(this, value);
                    }
                }

                this.Update(condition);
            }
            catch(Exception e)
            {
                return "-2";
            }
            return "1";
        }
        /// <summary>
        /// Deletes a this object in the SQL server.
        /// </summary>
        /// <param name="condition">
        /// is what condition you want to use for the SQL query to know what row to affect. For example "ID = 1".
        /// </param>

        public virtual void Delete(string condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();

                String query = "DELETE FROM " + databasePrefix + this.table + " WHERE " + ParameterizeCondition(condition, ref command);

                command.CommandText = query;

                command.Prepare();

                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Inserts this object into the SQL server.
        /// </summary>
        public virtual void Insert()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "INSERT INTO " + databasePrefix + this.table + " (";

                foreach (PropertyInfo pi in this.propertyInfos)
                {
                    if (!pi.Name.StartsWith("_") && !pi.Name.Equals("ID_PK"))
                        query += pi.Name + ",";             
                }
                query = query.Remove(query.Length - 1);
                query += ") VALUES (";
                foreach (PropertyInfo pi in this.propertyInfos)
                {
                    if (pi.Name.ToLower() == "ssma_timestamp")
                        query += "DEFAULT,";
                    else if (!pi.Name.StartsWith("_") && !pi.Name.Equals("ID_PK"))
                        query += "@" + pi.Name + ",";
                }
                query = query.Remove(query.Length - 1);
                query += ")";

                SqlCommand command = new SqlCommand(query, connection);

                foreach (PropertyInfo pi in this.PropertyInfos)
                {
                    if (!pi.Name.StartsWith("_") && pi.Name.ToLower() != "ssma_timestamp" && !pi.Name.Equals("ID_PK"))
                    {
                        Type t = pi.GetType();
                        int length = -1;

                        SqlDbType sdt = SqlDbType.NVarChar;
                        if (pi.PropertyType == typeof(DateTime?))
                            sdt = SqlDbType.DateTime;
                        else if (pi.PropertyType == typeof(Decimal?))
                            sdt = SqlDbType.Money;
                        else if (pi.PropertyType == typeof(bool))
                            sdt = SqlDbType.Bit;

                        SqlParameter sp = new SqlParameter("@" + pi.Name, sdt, length);

                        if (pi.GetType() == typeof(String))
                            sp.Value = AntiXssEncoder.HtmlEncode((String)pi.GetValue(this), false);
                        else
                            sp.Value = pi.GetValue(this) ?? DBNull.Value;

                        command.Parameters.Add(sp);
                    }
                    //command.Parameters.AddWithValue("@" + pi.Name, pi.GetValue(this));
                }


                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Selects a given object in the SQL server and parse it to this object.
        /// </summary>
        /// <param name="condition">
        /// is what condition you want to use for the SQL query to know what row to affect. For example "ID = 1".
        /// </param>
        public virtual bool Select(String condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String ssma = "";
                if (this.HasProperty("SSMA_timestamp"))
                    ssma = ", CAST(SSMA_timestamp AS BIGINT) AS ads";

                SqlCommand command = connection.CreateCommand();
                String query = @"SELECT *" + ssma + " FROM " + databasePrefix + this.table + " WHERE " + ParameterizeCondition(condition, ref command);

                command.CommandText = query;

                command.Prepare();
                bool hasRows = true;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        hasRows = false;
                    while (reader.Read())
                    {
                        int j = 0;
                        for (int i = 0; i < this.propertyInfos.Length; i++)
                        {
                            if (reader.GetName(j).ToLower() != "ssma_timestamp")
                            {
                                if(this.propertyInfos[i].PropertyType != typeof(ICollection<>))
                                {
                                    this.SetValue(this.propertyInfos[i].Name, reader[j]);
                                    j++;
                                }
                            }                         
                        }
                    }
                }
                return hasRows;
            }
        }

        /// <summary>
        ///     Takes a condition written as a string, for example Id = 1 AND Customer = 'test' OR valid = 'false',
        ///     And rewrite it to Sqlparameters to avoid injection
        /// </summary>
        /// <param name="condition">The condition that you want for your query to be injection safe</param>
        /// <param name="command">The Sqlcommand you want to hold the Sqlparameters</param>
        /// <returns>Returns a new string that will be ready for Sqlparameters</returns>
        private String ParameterizeCondition(String condition, ref SqlCommand command)
        {
            List<String> conditions = new List<String>(condition.Split(new String[] { " AND ", " OR " }, StringSplitOptions.None));
            Dictionary<String, SqlParameter> keyValue = new Dictionary<String, SqlParameter>();

            foreach(String equals in conditions)
            {
                if(equals.EndsWith("'") || equals.Contains("="))
                {
                    String key = equals.Split(new String[] { "=", "LIKE" }, StringSplitOptions.None)[0];
                    String value = equals.Split(new String[] { "=", "LIKE" }, StringSplitOptions.None)[1];

                    if (key.EndsWith(" "))
                        key = key.Remove(key.Length - 1);
                    if (value.StartsWith(" "))
                        value = value.Remove(0,1);
                    if (value[0] == '\'' && value[value.Length - 1] == '\'')
                        value = value.Remove(0,1).Remove(value.Length - 2);


                    condition = condition.Replace(equals, key + " = " + "@condition123_" + key);

                    
                    SqlDbType dbtype = SqlDbType.NVarChar;
                    if (key.ToLower() == "ssma_timestamp")
                        dbtype = SqlDbType.Int;

                    SqlParameter param = new SqlParameter("@condition123_" + key, dbtype, -1);
                    param.Value = value;

                    command.Parameters.Add(param);
                }
            }
            return condition;
        }

        /// <summary>
        /// checks if a given name of a property exists
        /// </summary>
        /// <param name="name">
        /// the name of the property you want to check if it exist
        /// </param>
        /// <returns>
        /// True if this current object has a property by the given name, else false
        /// </returns>
        private bool HasProperty(String name)
        {
            foreach(PropertyInfo pi in this.GetType().GetProperties())
            {
                if (pi.Name == name)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns a property as a valid text, No underlines ("_")
        /// </summary>
        /// <param name="name">
        /// Name of the property
        /// </param>
        /// <returns>
        /// returns the given property name as text
        /// </returns>
        public String AsText(String name)
        {
            return name.Replace("_", " ");
        }
    }
}
