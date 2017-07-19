using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper
{
    internal interface IRissoleProvider
    {
        RissoleTable GetRissoleTable<T>();
    }
}
