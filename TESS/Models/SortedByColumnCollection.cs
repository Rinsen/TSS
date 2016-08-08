using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TietoCRM.Models
{
    public class SortedByColumnCollection
    {
        private IEnumerable<Dictionary<String, object>> collection;
        public IEnumerable<Dictionary<String, object>> Collection
        {
            get
            {
                return collection;
            }
        }

        public SortedByColumnCollection(IEnumerable<Dictionary<String, object>> collection, String sortDirection, String sortKey)
        {
            this.collection = collection;
            //this.Initializer(collection);
            this.Sort(sortDirection, sortKey);
        }

        public SortedByColumnCollection(IEnumerable<SQLBaseClass> collection, String sortDirection, String sortKey)
        {
            // Convert List<SQLBaseClass> to List<Dictionary<String, object>
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(collection.First().GetType());
            List<Dictionary<String, object>> transferList = new List<Dictionary<String, object>>();
            foreach (var view in collection)
            {
                Dictionary<String, object> transferDictionary = new Dictionary<String, object>();
                foreach (PropertyDescriptor prop in props)
                {
                    transferDictionary.Add(prop.Name, prop.GetValue(view));
                }
                transferList.Add(transferDictionary);
            }
            this.collection = transferList;
            this.Sort(sortDirection, sortKey);
        }

        /*private void Initializer(IEnumerable<T> collection)
        {
            if(typeof(SQLBaseClass).IsAssignableFrom(typeof(T)))
            {
                // Convert List<SQLBaseClass> to List<Dictionary<String, object>
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(collection.First().GetType());
                List<Dictionary<String, object>> transferList = new List<Dictionary<String, object>>();
                foreach(var view in collection)
                {
                    Dictionary<String, object> transferDictionary = new Dictionary<String, object>();
                    foreach(PropertyDescriptor prop in props)
                    {
                        transferDictionary.Add(prop.Name, prop.GetValue(view));
                    }
                    transferList.Add(transferDictionary);
                }
                this.collection = transferList;
            }
            else if (typeof(T) == typeof(Dictionary<String, object>))
            {            
                this.collection = (List<Dictionary<String, object>>)collection;
            }
            else
            {
                throw new Exception("Unsupported collection: " + collection.GetType());
            }
        }*/


        private void Sort(String sortDirection, String sortKey)
        {
            if(this.collection.First().ContainsKey(sortKey))
            {
                if(sortDirection == "asc")
                {
                    this.collection = this.collection.OrderBy(x => x[sortKey]).ToList();
                }
                else if(sortDirection == "desc")
                {
                    this.collection = this.collection.OrderByDescending(x => x[sortKey]).ToList();
                }
                else
                {
                    throw new Exception("Unknown sort direction: " + sortDirection);
                }
            }
            else
            {
                throw new Exception("Unknown property: " + sortKey);
            }
        }

    }
}