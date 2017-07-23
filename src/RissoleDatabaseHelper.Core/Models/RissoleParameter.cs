using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleParameter
    {
        public RissoleParameter(string columnName, string parameterName, object value)
            :this(parameterName, value)
        {
            ColumnName = columnName;
        }
        public RissoleParameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
        }
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public object Value { get; set; }
    }
}
