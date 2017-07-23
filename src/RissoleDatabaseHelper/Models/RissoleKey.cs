using RissoleDatabaseHelper.Core.Attributes;
using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleKey
    {
        public RissoleKey() { }
        public RissoleKey(KeyAttribute keyAttribute)
        {
            Table = keyAttribute.Table;
            Column = keyAttribute.Column;
            Type = keyAttribute.Type;
        }

        public string Table { get; set; }
        public string Column { get; set; }
        public KeyType Type { get; set; }
    }
}
