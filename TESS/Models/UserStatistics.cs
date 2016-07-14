using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class UserStatistics : Statistics
    {
        public class StatisticsException:Exception
        {
            public StatisticsException(String message):base(message)
            {

            }
        }
        private view_User user;
        public view_User User
        {
            get
            {
                return user;
            }
        }

        public int OpenOffers
        {
            get
            {
                return this.getAmountOpenOffers();
            }
        }

        public int SentContracts
        {
            get
            {
                return this.getAmounSentContracts();
            }
        }

        public int ExpiringContracts
        {
            get
            {
                return this.getAmountExpiringContracts();
            }
        }

        private Dictionary<String, Object> cachedData = new Dictionary<string, object>();

        private List<String> customerNames;
        private List<view_Contract> contracts = new List<view_Contract>();

        /// <summary>
        /// By default this class dont use cached data
        /// </summary>
        /// <param name="userSign">The users sign to get the user from the sql server and then te correct statistics</param>
        public UserStatistics(String userSign):this(userSign, false) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSign">The users sign to get the user from the sql server and then te correct statistics</param>
        /// <param name="useCachedData">Whether the object should generate new data or get cached data from the SQL server</param>
        public UserStatistics(String userSign, bool useCachedData)
        {
            this.user = new view_User();
            if (!this.User.Select("Sign=" + userSign))
                throw new StatisticsException("The user sign given was incorrect and found no result when trying to fetch the user from sql server");
            this.initiateClass(this.user, useCachedData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user">The users to get correct statistics</param>
        public UserStatistics(view_User user) : this(user, false) { }

        /// <summary>
        /// By default this class dont use cached data
        /// </summary>
        /// <param name="user">The users to get correct statistics</param>
        /// <param name="useCachedData">Whether the object should generate new data or get cached data from the SQL server</param>
        public UserStatistics(view_User user, bool useCachedData)
        {
            this.initiateClass(user, useCachedData);
        }

        /// <summary>
        /// initiate the class with vitial data such as which customers to use or if it should use cached data
        /// </summary>
        /// <param name="user">The user which this statistics object should specify on</param>
        /// <param name="userCachedData"> whether to use cached data from SQL server or generate new data</param>
        private void initiateClass(view_User user, bool useCachedData)
        {
            this.user = user;
            this.useCachedData = useCachedData;
            if (!this.UseCachedData)
            {
                if (user.User_level > 1)
                    this.customerNames = view_Customer.getCustomerNames(this.User.Sign);
                else
                    this.customerNames = view_Customer.getCustomerNames();

                foreach (String name in this.customerNames)
                {
                    this.contracts.AddRange(view_Contract.GetContracts(name));
                }
            }
            else
                this.GetCachedData();
        }

        private int getAmountOpenOffers()
        {
            int amountOpenOffers = 0;
            if (!UseCachedData)
            {
                foreach (String customer in this.customerNames)
                {
                    amountOpenOffers += view_CustomerOffer.getAllCustomerOffers(customer).Where(m => m.Offer_status == "Öppen" && this.User.IfSameArea(m.Area)).ToList().Count;
                }
            }
            else
                amountOpenOffers = (int)cachedData["Open_offers"];

            return amountOpenOffers;
        }
        private int getAmounSentContracts()
        {
            int amountSentContracts;
            if (!UseCachedData)
                amountSentContracts = contracts.Where(m => m.Status == "Sänt" && this.User.IfSameArea(m.Area)).ToList().Count;
            else
                amountSentContracts = (int)cachedData["Sent_contracts"];

            return amountSentContracts;
        }

        private int getAmountExpiringContracts()
        {
            int amountExpiringContracts = 0;
            if (!UseCachedData)
            {
                foreach (view_Contract contract in contracts.Where(m => m.Status == "Giltigt").ToList())
                {
                    if (contract.Valid_through != null && (contract.Valid_through.Value - DateTime.Now).TotalDays <= 30
                        && this.User.IfSameArea(contract.Area))
                    {
                        amountExpiringContracts += 1;
                    }
                }
            }
            else
                amountExpiringContracts = (int)cachedData["Expiring_contracts"];

            return amountExpiringContracts;
        }

        /// <summary>
        /// Gets the latest data from the SQL server where theres a match with the objects user
        /// </summary>
        /// <returns>Returns true if it could fetch data, otherwise false</returns>
        protected override bool GetCachedData()
        {
            cachedData.Clear();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String query = "SELECT * FROM " + "dbo.view_" + "Statistics WHERE Sign='" + this.User.Sign + "'";

                SqlCommand command = new SqlCommand(query, connection);

                command.Prepare();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int i = 2;
                        while (reader.FieldCount > i)
                        {
                            cachedData.Add(reader.GetName(i), reader.GetValue(i));
                            i++;
                        }
                    }
                }
            }
            if (cachedData.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Updates, or insert if not found, the current users statistics to the SQL server
        /// </summary>
        public override void UpdateToSQLServer()
        {
            int oo = this.getAmountOpenOffers();
            int ec = this.getAmountExpiringContracts();
            int sc = this.getAmounSentContracts();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DataBaseCon"].ConnectionString))
            {
                connection.Open();

                String updateQuery = @"UPDATE dbo.view_Statistics SET Open_offers=@oo,Expiring_contracts=@ec,Sent_contracts=@sc WHERE Sign='" + this.User.Sign + @"'
                    IF @@ROWCOUNT=0
                    INSERT INTO dbo.view_Statistics (Sign,Open_offers,Expiring_contracts,Sent_contracts) VALUES(@sign,@oo,@ec,@sc)";

                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);

                updateCommand.Prepare();
                updateCommand.Parameters.AddWithValue("@sign", this.User.Sign);
                updateCommand.Parameters.AddWithValue("@oo", oo);
                updateCommand.Parameters.AddWithValue("@ec", ec);
                updateCommand.Parameters.AddWithValue("@sc", sc);
                updateCommand.ExecuteNonQuery();
            }

            if(UseCachedData)
            {
                cachedData["Open_offers"] = oo;
                cachedData["Expiring_contracts"] = ec;
                cachedData["Sent_contracts"] = sc;
            }
        }

        /// <summary>
        /// Updates, or insert if not found, all users to the SQL server
        /// </summary>
        public static void UpdateAllToSQLServer()
        {
            List<view_User> users = view_User.getAllUsers();

            foreach(view_User user in users)
            {
                UserStatistics stats = new UserStatistics(user, false);
                stats.UpdateToSQLServer();
            }
        }
    }
}