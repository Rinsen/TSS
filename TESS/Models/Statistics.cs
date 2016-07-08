using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class Statistics
    {
        private view_User user;
        public view_User User
        {
            get
            {
                return user;
            }
        }

        private List<String> customerNames;
        private List<view_Contract> contracts = new List<view_Contract>();
        public Statistics(String user)
        {
            this.user = new view_User();
            this.User.Select("Sign=" + user);
            this.customerNames = view_Customer.getCustomerNames(this.User.Sign);
            this.contracts = view_Contract.GetValidContracts(user);
        }
        public Statistics(view_User user)
        {
            this.user = user;
            this.customerNames = view_Customer.getCustomerNames(this.User.Sign);
            foreach(String name in this.customerNames)
            {
                this.contracts.AddRange(view_Contract.GetContracts(name));
            }
            
        }

        public int getAmountOpenOffers()
        {
            int amountOpenOffers = 0;
            foreach (String customer in this.customerNames)
            {
                amountOpenOffers += view_CustomerOffer.getAllCustomerOffers(customer).Where(m => m.Offer_status == "Öppen" && this.User.IfSameArea(m.Area)).ToList().Count;
            }
            return amountOpenOffers;
        }
        public int getAmounSentContracts()
        {
            int amountSentContracts = contracts.Where(m => m.Status == "Sänt" && this.User.IfSameArea(m.Area)).ToList().Count;
            return amountSentContracts;
        }

        public int getAmountExpiringContracts()
        {
            int amountExpiringContracts = 0;
            foreach (view_Contract contract in contracts.Where(m => m.Status == "Giltigt").ToList())
            {
                if (contract.Valid_through != null && (contract.Valid_through.Value - DateTime.Now).TotalDays <= 30 
                    && this.User.IfSameArea(contract.Area))
                {
                    amountExpiringContracts += 1;
                }
            }
            return amountExpiringContracts;
        }
    }
}