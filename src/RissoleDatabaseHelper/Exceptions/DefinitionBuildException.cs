using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Exceptions
{
    public class DefinitionBuildException : Exception
    {
        public  DefinitionBuildException()
            : base() { }

        public  DefinitionBuildException(string message)
            : base(message) { }

        public  DefinitionBuildException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public  DefinitionBuildException(string message, Exception innerException)
            : base(message, innerException) { }

        public  DefinitionBuildException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
