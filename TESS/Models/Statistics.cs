using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public abstract class Statistics
    {
        public abstract void UpdateToSQLServer();

        protected abstract bool GetCachedData();

        protected bool useCachedData = false;
        public bool UseCachedData
        {
            get
            {
                return useCachedData;
            }
        }
    }
        
}