using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    internal class RissoleScript
    {
        public RissoleScript() {
            Parameters = new List<RissoleParameter>();
        }

        public RissoleScript(string script, List<List<RissoleParameter>> parametersGroup) 
            : this()
        {
            Script = script;
            foreach (var parameters in parametersGroup)
            {
                Parameters.AddRange(parameters);
            }
        }

        public RissoleScript(string script, params List<RissoleParameter>[] parameters)
            : this(script, parameters.ToList()) { }

        public string Script { get; set; }

        public List<RissoleParameter> Parameters { get; set; }
    }
}
