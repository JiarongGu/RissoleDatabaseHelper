using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
