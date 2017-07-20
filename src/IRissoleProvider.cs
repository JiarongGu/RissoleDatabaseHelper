using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper
{
    internal interface IRissoleProvider
    {
        RissoleTable GetRissoleTable<T>();

        RissoleScript GetWhereCondition<T>(Expression<Func<T, bool>> expression);

        RissoleScript GetJoinScript<T, TJoin>(Expression<Func<T, TJoin, bool>> expression);

        RissoleScript GetSelectCondition<T>(Expression<Func<T, object>> expression);

        RissoleCommandExecutor<T> GetRissoleExecutor<T>();

        RissoleCommandExecutor<T> GetRissoleExecutor<T>(IDbConnection dbConnection);
    }
}
