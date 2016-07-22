using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TietoCRM.Models
{
    public abstract class HashtagDocument : SQLBaseClass
    {
        private int _id;
        public virtual int _ID { get; set; }

        private List<String> _hashtagList = new List<String>();
        public List<String> _HashtagList
        {
            get
            {
                return this._hashtagList;
            }
        }

        public HashtagDocument(string table) : base(table)
        {

        }

        public void ParseHashtags(String hashtags)
        {
            this._hashtagList.Clear();
            if(!String.IsNullOrEmpty(hashtags))
            {
                while (hashtags.Contains(" "))
                    hashtags = hashtags.Replace(" ", "");

                if (hashtags[0] == '#')
                    hashtags = hashtags.Remove(0, 1);
                else
                    throw new System.Data.SyntaxErrorException("first tag was formated wrong, something with no starting hashtag, typical hashtag looks like this #yolo");

                String[] tags = hashtags.Split('#');

                Regex regex = new Regex(@"^\w+$");
                foreach (String tag in tags)
                {
                    if (regex.IsMatch(tag))
                        this._HashtagList.Add(tag);
                    else
                        throw new System.Data.SyntaxErrorException("One of the tags (" + tag + ") was formated wrong, typical hashtag looks like this #yolo");
                }
            }
        }

        public override void Update(string condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                this.DeleteHashtag(connection);

                foreach (String hashtag in this._HashtagList)
                {
                    if (hashtag != null)
                    {
                        this.InsertHashtag(connection, hashtag);
                    }
                }
            }
            base.Update(condition);
        }

        public override int Insert()
        {
            int id = base.Insert();
            this._ID = id;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (String hashtag in this._HashtagList)
                {
                    if (hashtag != null)
                    {
                        this.InsertHashtag(connection, hashtag);
                    }
                }
            }
            return id;
        }

        public override void Delete(string condition)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                this.DeleteHashtag(connection);
            }
                base.Delete(condition);
        }

        public override bool Select(string condition)
        {
            bool didSucceed = base.Select(condition);
            if (didSucceed)
            {
                this._hashtagList = this.GetHashtags();
                return true;
            }
            else
                return false;
        }

        public String HashtagsAsString()
        {
            String tags = "";
            foreach(String tag in this._HashtagList)
            {
                tags += "#" + tag + " ";
            }
            if (tags.Length > 0)
                return tags.Remove(tags.Length - 1, 1);
            else
                return "";
        }

        public List<String> GetHashtags()
        {
            List<String> list = new List<String>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                String query = "SELECT Hashtag FROM " + databasePrefix + "Hashtag WHERE Document_id=@id AND SQL_table=@table";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                command.Parameters.AddWithValue("@id", this._ID);
                command.Parameters.AddWithValue("@table", this.GetType().ToString());

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            this._hashtagList = list;
            return list;
        }

        private void DeleteHashtag(SqlConnection connection)
        {
            String deleteQuery = "DELETE FROM " + databasePrefix + "Hashtag WHERE Document_id=@id AND SQL_table=@table";

            SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);

            deleteCommand.Prepare();
            deleteCommand.Parameters.AddWithValue("@id", this._ID);
            deleteCommand.Parameters.AddWithValue("@table", this.GetType().ToString());
            deleteCommand.ExecuteNonQuery();
        }

        private void InsertHashtag(SqlConnection connection, String hashtag)
        {
            String insertQuery = "INSERT INTO " + databasePrefix + "Hashtag (Document_id,SQL_table,Hashtag) VALUES(@did,@table,@ht)";

            SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

            insertCommand.Prepare();
            insertCommand.Parameters.AddWithValue("@did", this._ID);
            insertCommand.Parameters.AddWithValue("@table", this.GetType().ToString());
            insertCommand.Parameters.AddWithValue("@ht", hashtag);
            insertCommand.ExecuteNonQuery();
        }
    }
}