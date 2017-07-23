using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute() { }
    }
}
