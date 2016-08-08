using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public abstract class Statistics
    {
        public class StatisticsException : Exception
        {
            private SQLBaseClass viewModel;
            public SQLBaseClass ViewModel
            {
                get
                {
                    return viewModel;
                }
            }
            public StatisticsException(String message, SQLBaseClass viewModel) : base(message)
            {
                this.viewModel = viewModel;
            }

            public StatisticsException(String message) : base(message)
            {
                this.viewModel = viewModel;
                this.viewModel = null;
            }
        }

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