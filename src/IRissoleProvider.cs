using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    internal interface IRissoleProvider
    {
        RissoleTable GetRissoleTable<T>();

        RissoleCommandExecutor<T> GetRissoleExecutor<T>();

        RissoleCommandExecutor<T> GetRissoleExecutor<T>(IDbConnection dbConnection);
    }
}
