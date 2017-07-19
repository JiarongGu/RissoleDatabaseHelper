using RissoleDatabaseHelper.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute() { }

        public KeyAttribute(string table, string column, KeyType type)
        {
            Table = table;
            Column = column;
            Type = type;
        }

        public string Table { get; set; }

        public string Column { get; set; }

        public KeyType Type { get; set; }
    }
}
