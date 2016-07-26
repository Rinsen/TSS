using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TietoCRM.Models
{
    public class SortedByColumnCollection<T>
    {
        private List<Dictionary<String, object>> collection;
        public List<Dictionary<String, object>> Collection
        {
            get
            {
                return collection;
            }
        }

        public SortedByColumnCollection(IEnumerable<T> collection, String sortDirection, String sortKey)
        {
            this.Initializer(collection);
            this.Sort(sortDirection, sortKey);
        }

        private void Initializer(IEnumerable<T> collection)
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
        }


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