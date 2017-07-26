using RissoleDatabaseHelper.Core.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    public interface IRissoleEntity<T>
    {
        // properties
        IDbConnection Connection { get; set; }

        // functions
        IRissoleBaseCommand<T> Custom(string script);

        IRissoleBaseCommand<T> Custom(string script, List<IDbDataParameter> parameters);

        IRissoleBaseCommand<T> Custom(string script, params IDbDataParameter[] parameters);
        
        IRissoleBaseCommand<T> Select(Expression<Func<T, object>> prdicate);

        IRissoleBaseCommand<T> First(Expression<Func<T, object>> prdicate);

        IRissoleInsertCommand<T> Insert(T model);

        IRissoleBaseCommand<T> Delete(Expression<Func<T, bool>> prdicate);

        IRissoleBaseCommand<T> Delete(T model);

        IRissoleBaseCommand<T> Update(T model, bool includePirmaryKey = false);

        IRissoleBaseCommand<T> Update(IList<T> models, bool includePirmaryKey = false);
    }
}
