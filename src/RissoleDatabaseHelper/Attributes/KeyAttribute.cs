using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute() { }

        public KeyAttribute(KeyType type)
        {
            Type = type;
        }
        
        public KeyAttribute(string table, string column, KeyType type) :
            this(type)
        {
            Table = table;
            Column = column;
        }

        public string Table { get; set; }

        public string Column { get; set; }

        public KeyType Type { get; set; }
    }
}
