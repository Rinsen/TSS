using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TietoCRM.Models
{
    public static class Cloner<T>
    {

        /// <summary>
        /// This method deepclones datacontract items.
        /// </summary>
        /// <param name="a">Object to clone</param>
        /// <returns>copy of item</returns>
        public static T DeepClone(object a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                dcs.WriteObject(stream, a);
                stream.Position = 0;
                return (T)dcs.ReadObject(stream);

            }
        }

    }
}