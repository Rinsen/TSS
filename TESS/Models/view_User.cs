using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TietoCRM.Models
{
    public class view_User : SQLBaseClass
	{

		private String sign;
		public String Sign { get{ return sign; } set{ sign = value; } }

		private String name;
		public String Name { get{ return name; } set{ name = value; } }

        private String area;
        public String Area { get { return area; } set { area = value; } }

        private String offer_file_location;
		public String Offer_file_location { get{ return offer_file_location; } set{ offer_file_location = value; } }

        private String contract_file_location;
        public String Contract_file_location { get { return contract_file_location; } set { contract_file_location = value; } }

        private String file_format;
		public String File_format { get{ return file_format; } set{ file_format = value; } }

		private String address;
		public String Address { get{ return address; } set{ address = value; } }

		private String city;
		public String City { get{ return city; } set{ city = value; } }

		private String telephone;
		public String Telephone { get{ return telephone; } set{ telephone = value; } }

		private String mobile;
		public String Mobile { get{ return mobile; } set{ mobile = value; } }

        private String windows_user;
        public String Windows_user { get { return windows_user; } set { windows_user = value; } }

        private decimal user_level;
        public decimal User_level { get { return user_level; } set { user_level = value; } }

        private bool use_logo;
        public bool Use_logo { get { return use_logo; } set { use_logo = value; } }

        public view_User() : base("User")
		{
			//ctr
		}

        public override void Insert()
        {
            base.Insert();
            UserStatistics stats = new UserStatistics(this);
            stats.UpdateToSQLServer();
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_User> getAllUsers()
        {
            List<view_User> list = new List<view_User>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT * FROM " + databasePrefix + "User";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                            view_User k = new view_User();
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
        
        public bool IfSameArea(String area)
        {
            return area == this.Area || this.Area == "*";
        } 

    }

}