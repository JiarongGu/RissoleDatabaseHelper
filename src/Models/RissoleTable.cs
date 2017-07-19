using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace RissoleDatabaseHelper.Models
{
    internal class RissoleTable
    {
        public RissoleTable() { }

        public RissoleTable(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public List<RissoleColumn> Columns { get; set; }

        public RissoleColumn GetColumnByPropertyName(string name)
        {
            return Columns.FirstOrDefault(x => x.Property.Name == name);
        }
    }
}
