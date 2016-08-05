using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using TietoCRM.Models;

namespace TietoCRM.Models
{
    public class FileLocationMapping
    {
        private HashtagDocument document;

        public HashtagDocument Document
        {
            get
            {
                return document;
            }
        }

        public view_User User
        {
            get
            {
                return user;
            }        
        }

        private view_User user;

        public FileLocationMapping(view_User User, view_CustomerOffer CustomerOffer)
        {
            this.user = User;
            this.document = CustomerOffer;
        }

        public FileLocationMapping(view_User User, view_Contract Contract)
        {
            this.user = User;
            this.document = Contract;
        }


        private string CombineFilePath(Dictionary<String,String> LocationMappingDic)
        {
            String returnString = "";

            if (Document.GetType() == typeof(view_CustomerOffer))
                returnString = this.User.Offer_file_location;
            else
                returnString = this.User.Contract_file_location;
            int pos = 0;
            foreach (KeyValuePair<String,String> Mapping in LocationMappingDic)
            {
                int temp = returnString.IndexOf(Mapping.Key, pos);

                String modifiedString = returnString.Substring(0, pos);
                var regx = new Regex(Regex.Escape(Mapping.Key),RegexOptions.IgnoreCase);
                int endPos = returnString.IndexOf(Mapping.Key, pos) + Mapping.Key.Length;

                String sub = returnString.Substring(pos, endPos - pos);
                String unmodifiedString = returnString.Substring(temp+Mapping.Key.Length, returnString.Length - temp - Mapping.Key.Length);
                String middleString = regx.Replace(sub, Mapping.Value, 1);

                returnString = modifiedString + middleString + unmodifiedString;
                pos = (modifiedString + middleString).Length;
            }
            return returnString;
        }

        private List<String> SplitLocationString()
        {
            List<String> returnList;

            // Match any @Word or @Word(data)
            Regex Regx1 = new Regex(@"@[A-Za-z]+");
            if (Document.GetType() == typeof(view_CustomerOffer))
            {
                List<String> lst1 = Regx1.Matches(user.Offer_file_location).Cast<Match>().Select(match => match.Value).ToList();
                returnList = lst1;
            }
            else // view_Contract
            {
                returnList = Regx1.Matches(user.Contract_file_location).Cast<Match>().Select(match => match.Value).ToList();
            }

            return returnList;
        }

        public string GetFilePath()
        {
            List<String> MappingList = SplitLocationString();
            Dictionary<String,String> ReturnDic = new Dictionary<String,String>();

            foreach (String LocationMapping in MappingList)
            {
                String LM = LocationMapping.ToUpper();

                String ID = "";
                String Customer = "";
                String Title = "";
                String Type = "";
                if(Document.GetType() == typeof(view_CustomerOffer))
                {
                    ID = ((view_CustomerOffer)Document)._Offer_number.ToString();
                    Customer = ((view_CustomerOffer)Document).Customer;
                    Title = ((view_CustomerOffer)Document).Title;
                }
                else // contract
                {
                    ID = ((view_Contract)Document).Contract_id.ToString();
                    Customer = ((view_Contract)Document).Customer;
                    Title = ((view_Contract)Document).Title;
                    Type = ((view_Contract)Document).Contract_type;
                }

                if (LM == "@ID" || LM == "@NR")
                    ReturnDic.Add(LM, ID ?? "");
                else if (LM == "@DATE")
                    ReturnDic.Add(LM, DateTime.Now.ToString("yyyy-MM-dd"));
                /* else if (FormatedDateRegex.IsMatch(LocationMapping))
                 {
                     String DateFormat = FormatedDateRegex.Match(LocationMapping).Groups[1].Value.ToString();
                     ReturnDic.Add(LocationMapping, DateTime.Now.ToString(DateFormat));
                 }*/
                else if (LM == "@REP" || LM == "@REPRESENTATIVE")
                {
                    view_Customer vCustomer = new view_Customer("Customer = " + Customer);
                    ReturnDic.Add(LM, vCustomer.GetReprensentativesAsString() ?? "");
                }
                else if (LM == "@TITLE")
                    ReturnDic.Add(LM, Title ?? "");
                else if (LM == "@CUSTOMER")
                    ReturnDic.Add(LM, Customer ?? "");
                else if (LM == "@TYPE")
                {
                    if (Document.GetType() == typeof(view_Contract))
                        ReturnDic.Add(LM, Type ?? "");
                }
                     

            }

            return CombineFilePath(ReturnDic);
        }
    }
}