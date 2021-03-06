﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }

        public ColumnAttribute(string name)
        {
            Name = name;
        }

        //column name
        public string Name { get; set; }

        //data type
        public Type DataType { get; set; }

        //is database generated
        public bool IsGenerated { get; set; }
        
        //executor generated
        public bool IsComputed { get; set; }
    }
}
