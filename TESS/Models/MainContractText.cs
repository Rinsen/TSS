using System;
using System.Collections.Generic;

namespace TietoCRM.Models
{
    /// <summary>
    /// Denna klass används inte längre!
    /// </summary>
    public class MainContractText
    {
        public enum MainContractType
        {
            MainHead,
            Subheading,
            Text
        };

        private MainContractType type;
        public MainContractType Type
        {
            get { return type; }
            set { type = value; }
        }

        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private String value;
        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private List<MainContractText> children;
        public List<MainContractText> Children
        {
            get { return children; }
            set { children = value; }
        }

        /// <summary>
        /// init the object with certain values.
        /// </summary>
        /// <param name="name">The column name in the database</param>
        /// <param name="type">The heading type of the object, MainHead, Subheading, Text</param>
        /// <param name="value">The value the database holds</param>
        public MainContractText(String name, MainContractType type, String value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
            this.Children = new List<MainContractText>();
        }
    }
}