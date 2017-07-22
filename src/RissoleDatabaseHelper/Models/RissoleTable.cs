using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleTable
    {
        public RissoleTable() {
            Columns = new List<RissoleColumn>();
        }

        public RissoleTable(Type type)
            :this()
        {
            ReferenceType = type;
            Name = type.GetTypeInfo().Name;
        }

        public string Name { get; set; }

        public List<RissoleColumn> Columns { get; set; }

        public RissoleColumn GetColumnByPropertyName(string name)
        {
            return Columns.FirstOrDefault(x => x.Property.Name == name);
        }

        public Type ReferenceType { get;}
    }
}
