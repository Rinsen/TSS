using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class Statistics
    {
        private String user;
        public string User
        {
            get
            {
                return user;
            }
        }

        private List<String> customerNames;
        private List<view_Contract> contracts;
        public Statistics(String user)
        {
            this.user = user;
            this.customerNames = view_Customer.getCustomerNames(this.User);
            this.contracts = view_Contract.GetValidContracts(user);
        }

        public int getAmountOpenOffers()
        {
            int amountOpenOffers = 0;
            foreach (String customer in this.customerNames)
            {
                amountOpenOffers += view_CustomerOffer.getAllCustomerOffers(customer).Where(m => m.Offer_status == "Öppen").ToList().Count;
            }
            return amountOpenOffers;
        }
        public int getAmounSentContracts()
        {
            int amountSentContracts = contracts.Where(m => m.Status == "Sänt").ToList().Count;
            return amountSentContracts;
        }

        public int getAmountExpiringContracts()
        {
            int amountExpiringContracts = 0;
            foreach (view_Contract contract in contracts.Where(m => m.Status == "Giltigt").ToList())
            {
                if (contract.Valid_through.Value != null)
                {
                    if ((DateTime.Now - contract.Valid_through.Value).TotalDays <= 30)
                    {
                        amountExpiringContracts += 1;
                    }
                }
            }
            return amountExpiringContracts;
        }
    }
}