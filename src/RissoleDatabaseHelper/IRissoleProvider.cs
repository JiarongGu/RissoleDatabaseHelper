using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    internal interface IRissoleProvider
    {
        RissoleTable GetRissoleTable<T>();

        // with parameters
        RissoleScript GetWhereScript<T>(Expression<Func<T, bool>> expression, int stack);

        RissoleScript GetJoinScript<T, TJoin>(Expression<Func<T, TJoin, bool>> expression, int stack);
        
        RissoleScript GetPrimaryScript<T>(T model, int stack);

        RissoleScript GetSetValueScript<T>(T model, int stack, bool ignorePrimaryKey);

        // without parameters
        RissoleScript GetSelectScript<T>(Expression<Func<T, object>> expression);

        RissoleScript GetFirstScript<T>(Expression<Func<T, object>> expression);

        RissoleScript GetDeleteScript<T>();

        RissoleScript GetInsertScript<T>();

        RissoleScript GetUpdateScript<T>();
    }
}
