using RissoleDatabaseHelper.Enums;
using System;
using System.Text;

namespace RissoleDatabaseHelper.Models
{
    internal class RissoleKey
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public KeyType Type { get; set; }
        public bool IsComputed { get; set; }
    }
}
