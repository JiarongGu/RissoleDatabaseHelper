using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string table, string column)
        {
            Table = table;
            Column = column;
        }

        public string Table { get; set; }

        public string Column { get; set; }
    }
}
