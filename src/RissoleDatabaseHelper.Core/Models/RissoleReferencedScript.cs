using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Models
{
    /// <summary>
    /// query script which referenced by database types
    /// </summary>
    internal class RissoleReferencedScript
    {
        private string _actualScript;
        private string _trialScript;

        public RissoleReferencedScript(string actualScript)
        {
            _actualScript = actualScript;
        }

        public RissoleReferencedScript(string actualScript, string trialScript)
            : this(actualScript)
        {
            _trialScript = trialScript;
        }

        /// <summary>
        /// the actual script that need to be added in command
        /// </summary>
        public string ActualScript {
            get { return _actualScript; }
        }

        /// <summary>
        /// the script to test if the actual script is vaild for database
        /// </summary>
        public string TrialScript {
            get { return string.IsNullOrEmpty(_trialScript)? _actualScript : _trialScript; }
        }
    }
}
