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
        IRissoleCommand<T> Custom(string script, T model);

        IRissoleCommand<T> Custom(string script, List<IDbDataParameter> parameters);

        IRissoleCommand<T> Custom(string script, params IDbDataParameter[] parameters);

        IRissoleCommand<T> Insert(T model);

        IRissoleCommand<T> Insert(List<T> model);

        IRissoleCommand<T> Delete(Expression<Func<T, bool>> prdicate);

        IRissoleCommand<T> Delete(T model);

        IRissoleCommand<T> Delete(object primary);

        IRissoleCommand<T> Update(T model);

        IRissoleCommand<T> Update(IList<T> model);
        
        IRissoleCommand<T> Select(Expression<Func<T, object>> prdicate);

        IRissoleCommand<T> First(Expression<Func<T, object>> prdicate);
    }
}
