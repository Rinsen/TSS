using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class view_Information
    {
        int _id;
        public int _ID { get; set; }

        String title;
        public string Title { get; set; }

        String message;
        public String Message { get; set; }

        DateTime _created;
        public DateTime Created { get; set; }

        DateTime updated;
        public DateTime Updated { get; set; }


    }
}