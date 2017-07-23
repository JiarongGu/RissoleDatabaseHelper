using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Exceptions
{
    public class RissoleException : Exception
    {
        public  RissoleException()
            : base() { }

        public  RissoleException(string message)
            : base(message) { }

        public  RissoleException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public  RissoleException(string message, Exception innerException)
            : base(message, innerException) { }

        public  RissoleException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
