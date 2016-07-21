using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Information : SQLBaseClass
    {
        int _id;
        public int _ID { get; set; }

        String author;
        public String Author { get; set; }

        String title;
        public string Title { get; set; }

        String message;
        public String Message { get; set; }

        String type;
        public String Type { get; set; }

        DateTime _created;
        public DateTime Created { get; set; }

        DateTime updated;
        public DateTime Updated { get; set; }

        DateTime expires;
        public DateTime Expires { get; set; }

        public view_Information() : base("Information")
        {

        }

        /// <summary>
        /// Send the objects variables to the database. And automatically update the Updated field.
        /// </summary>
        /// <param name="condition">
        /// is what condition you want to use for the SQL query to know what row to affect. For example "ID = 1".
        /// </param>
        public override void Update(String condition)
        {
            this.Updated = DateTime.Now;
            base.Update(condition);
        }

        /// <summary>
        /// Inserts this object into the SQL server. And automatically inserts the Created and Updated field.
        /// </summary>
        public override int Insert()
        {
            this.Created = DateTime.Now;
            this.Updated = DateTime.Now;
            return base.Insert();
        }

        /// <summary>
        /// Gets all information.
        /// </summary>
        /// <returns>A lsit of users.</returns>
        public static List<view_Information> getAllInformation()
        {
            List<view_Information> list = new List<view_Information>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT [ID], Author, Title, Message, Type, Created, Updated, Expires FROM " + databasePrefix + "Information ";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        view_Information k = new view_Information();
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

        public static List<view_Information> getAllValidInformation()
        {
            List<view_Information> allInfo = view_Information.getAllInformation();
            allInfo = allInfo.FindAll(a => DateTime.Compare(a.Expires, DateTime.Now) >= 0);
            allInfo.Sort((a, b) => a.Updated.CompareTo(b.Updated));
            return allInfo;
        }

        public string getAuthorName()
        {
            view_User user = new view_User();
            user.Select("Sign = " + this.Author);
            return user.Name;
        }

    }
}