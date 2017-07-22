using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute() { }

        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
