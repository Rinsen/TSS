using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TietoCRM.Extensions;

namespace TietoCRM.Models
{
    public class view_Exception : SQLBaseClass
    {
        int _id;
        public int _ID { get; set; }

        String windows_user;
        public String Windows_user { get; set; }

        String method;
        public String Method { get; set; }

        String type;
        public String Type { get; set; }

        String hresult;
        public String HResult { get; set; }

        String message;
        public string Message { get; set; }

        String stacktrace;
        public String StackTrace { get; set; }


        DateTime _Created;
        public DateTime Created { get; set; }

        public view_Exception() : base("Exception")
        {
        }

        public override int Insert()
        {
            this.Created = DateTime.Now;
            return base.Insert();
        }

        public static void UploadException(Exception e)
        {
            view_Exception exp = new view_Exception();
            exp.Type = e.GetType().ToString();
            exp.Windows_user = System.Security.Principal.WindowsPrincipal.Current.Identity.Name;
            exp.Method = e.TargetSite.Name;
            exp.Message = e.Message;
            exp.HResult = e.HResult.ToString();
            exp.StackTrace = e.StackTrace;

            exp.Insert();

        }
    }
}