using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using TietoCRM.FeatureService;

namespace TietoCRM.Models
{
    public class FeatureServiceProxy
    {
        public static string ServiceUri { get; set; }
        public static IApplications GetApplicationClient()
        {
            var myChannelFactory = new ChannelFactory<IApplications>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IClient GetClientClient()
        {
            var myChannelFactory = new ChannelFactory<IClient>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IFeatures GetFeaturesClient()
        {
            ChannelFactory<IFeatures> myChannelFactory = null;
            if(ServiceUri != null)
            {
                myChannelFactory = new ChannelFactory<IFeatures>(GetBindning(), new EndpointAddress(ServiceUri));
                return myChannelFactory.CreateChannel();
            }

            return null;
        }

        public static ILicense GetLicenseClient()
        {
            var myChannelFactory = new ChannelFactory<ILicense>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static ILogg GetLoggClient()
        {
            var myChannelFactory = new ChannelFactory<ILogg>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }
        public static IProduct GetProductClient()
        {
            var myChannelFactory = new ChannelFactory<IProduct>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IScript GetScriptClient()
        {
            var myChannelFactory = new ChannelFactory<IScript>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IUser GetUserClient()
        {
            var myChannelFactory = new ChannelFactory<IUser>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IVersion GetVersionClient()
        {
            var myChannelFactory = new ChannelFactory<IVersion>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        public static IWriter GetWriterClient()
        {
            var myChannelFactory = new ChannelFactory<IWriter>(GetBindning(), new EndpointAddress(ServiceUri));
            return myChannelFactory.CreateChannel();
        }

        private static BasicHttpBinding GetBindning()
        {
            var myBinding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReceiveTimeout = new TimeSpan(0, 0, 0, 20),
                SendTimeout = new TimeSpan(0, 0, 0, 20),
            };
            return myBinding;
        }
    }
}