using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleColumn
    {
        public RissoleColumn() {
            Keys = new List<RissoleKey>();
        }

        public RissoleColumn(PropertyInfo property)
            :this()
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
        public bool IsComputed { set; get; }
        public List<RissoleKey> Keys { set; get;}
    }
}
