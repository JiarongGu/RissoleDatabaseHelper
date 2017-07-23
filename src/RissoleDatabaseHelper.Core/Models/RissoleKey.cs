using RissoleDatabaseHelper.Core.Attributes;
using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleKey
    {
        public RissoleKey(PrimaryKeyAttribute keyAttribute)
        {
            Type = KeyType.PrimaryKey;
        }

        public RissoleKey(ForeignKeyAttribute keyAttribute)
        {
            Table = keyAttribute.Table;
            Column = keyAttribute.Column;
            Type = KeyType.ForeignKey;
        }

        public string Table { get; set; }
        public string Column { get; set; }
        public KeyType Type { get; set; }
    }
}
