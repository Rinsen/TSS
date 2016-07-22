using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using TietoCRM.Models.Interfaces;

namespace TietoCRM.Models
{
    public class ViewCsvParser<T> where T : SQLBaseClass
    {
        public ViewCsvParser()
        {
        }


        public void WriteTsv(IEnumerable<SQLBaseClass> data, TextWriter output)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Write(prop.DisplayName); // header
                output.Write("\t");
            }
            output.WriteLine();
            foreach (SQLBaseClass item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Write(prop.Converter.ConvertToString(
                         prop.GetValue(item)));
                    output.Write("\t");
                }
                output.WriteLine();
            }
        }
    }
}