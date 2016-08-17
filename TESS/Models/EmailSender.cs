using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace TietoCRM.Models
{
    public class EmailSender
    {
        private view_User sender;
        public view_User Sender { get { return sender; }}

        private List<view_User> receivers;
        public List<view_User> Receivers { get { return receivers; } set { receivers = value; } }

        private DirectorySearcher searcher = new DirectorySearcher();

        public EmailSender(view_User sender)
        {
            this.sender = sender;
            this.receivers = new List<view_User>();
        }

        public EmailSender(view_User sender, List<view_User> receivers)
        {
            this.sender = sender;
            this.receivers = receivers;
        }

        public void Send(String title, String message)
        {
            if(this.Receivers.Count > 0)
            {
                searcher.Filter = string.Format("sAMAccountName={0}", Sender.Windows_user.Remove(0, Sender.Windows_user.IndexOf("\\") + 1));
                SearchResult thisUser = searcher.FindOne();
                string thisemailAddr = thisUser.Properties["mail"][0].ToString();

                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                mailMessage.From = new System.Net.Mail.MailAddress(WebConfigurationManager.AppSettings["emailSender"], this.Sender.Name);
                foreach (view_User user in Receivers)
                {
                    searcher.Filter = string.Format("sAMAccountName={0}", user.Windows_user.Remove(0, user.Windows_user.IndexOf("\\") + 1));
                    SearchResult searchUser = searcher.FindOne();
                    string emailAddr = searchUser.Properties["mail"][0].ToString();

                    mailMessage.To.Add(new System.Net.Mail.MailAddress(emailAddr));
                }

                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.Subject = title;
                mailMessage.Body = message;

                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                client.Host = WebConfigurationManager.AppSettings["emailRelay"];
                client.Port = 25;
                client.UseDefaultCredentials = true;
                client.EnableSsl = false;
                client.Send(mailMessage);
                mailMessage.Dispose();
                client.Dispose();
            }
            else
            {
                throw new SmtpException("No email address to send to");
            }
        }
    }
}