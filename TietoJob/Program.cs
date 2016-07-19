using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TietoCRM.Models;

namespace TietoJob
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!System.Diagnostics.EventLog.SourceExists("Tieto"))
                System.Diagnostics.EventLog.CreateEventSource("Tieto", "Job");
            //try
            //{
                CustomerStatistics.UpdateAllToSQLServer();
                Console.WriteLine("CustomerStatistics done");
                //UserStatistics.UpdateAllToSQLServer();
                Console.WriteLine("UserStatistics done");

            //}
            //catch (Exception e)
            //{
            //System.Diagnostics.EventLog eventlog = new System.Diagnostics.EventLog("Job", ".", "Tieto");
            //eventlog.WriteEntry(e.Message);
            //throw e;
            //}
        }
    }
}
