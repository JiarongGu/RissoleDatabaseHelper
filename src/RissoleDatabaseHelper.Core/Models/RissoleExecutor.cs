using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleExecutor
    {
        public Type AssembleType { get; set; }

        public RissoleTable Table { get; set; }

        public Dictionary<string, string> Scripts { get; set; }
    }
}
