using System;

namespace TietoCRM.UD_Exceptions
{
    [Serializable()]
    public class CustomerException : Exception
    {
        public CustomerException(string message) : base(message)
        {
        }

        public CustomerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CustomerException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        {
        }
    }
}