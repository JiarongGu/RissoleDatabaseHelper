using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper.Models
{
    internal class RissoleScript
    {
        public RissoleScript() {
            Parameters = new List<IDbDataParameter>();
        }

        public string Script { get; set; }

        public ICollection<IDbDataParameter> Parameters { get; set; }
    }
}
