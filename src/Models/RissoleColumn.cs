using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Models
{
    internal class RissoleColumn
    {
        public RissoleColumn() { }

        public RissoleColumn(PropertyInfo property)
        {
            Property = property;
            Name = property.Name;
            DataType = property.GetType();
            IsGenerated = false;
            IsComputed = false;
        }

        public PropertyInfo Property { set; get; }
        public string Name { set; get; }
        public Type DataType { set; get; }
        public bool IsGenerated { set; get; }
        public bool IsPrimaryKey { set; get; }
        public bool IsComputed { set; get; }
        public List<RissoleKey> Keys { set; get;}
    }
}
