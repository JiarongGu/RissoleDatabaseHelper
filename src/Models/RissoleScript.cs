using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper.Models
{
    internal class RissoleScript
    {
        public RissoleScript() {
            Parameters = new Dictionary<string, object>();
        }

        public RissoleScript(string script, params Dictionary<string, object>[] parameters) :this()
        {
            Script = script;
            foreach (var parameterDisctionary in parameters)
            {
                foreach (var parameter in parameterDisctionary)
                {
                    Parameters.Add(parameter.Key, parameter.Value);
                }
            }
        }

        public string Script { get; set; }

        public Dictionary<string ,object> Parameters { get; set; }
    }
}
