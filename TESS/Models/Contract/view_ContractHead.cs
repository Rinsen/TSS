using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_ContractHead : SQLBaseClass
    {
        private String contract_id;
        public String Contract_id { get { return contract_id; } set { contract_id = value; } }

        private String customer;
        public String Customer { get { return customer; } set { customer = value; } }

        private String buyer;
        public String Buyer { get { return buyer; } set { buyer = value; } }

        private String contact_person;
        public String Contact_person { get { return contact_person; } set { contact_person = value; } }

        private String address;
        public String Address { get { return address; } set { address = value; } }

        private String zip_code;
        public String Zip_code { get { return zip_code; } set { zip_code = value; } }

        private String city;
        public String City { get { return city; } set { city = value; } }

        private String corporate_identity_number;
        public String Corporate_identity_number { get { return corporate_identity_number; } set { corporate_identity_number = value; } }

        private String customer_sign;
        public String Customer_sign { get { return customer_sign; } set { customer_sign = value; } }

        private String our_sign;
        public String Our_sign { get { return our_sign; } set { our_sign = value; } }

        public view_ContractHead()
            : base("ContractHead")
        {
            //ctr
        }
    }
}