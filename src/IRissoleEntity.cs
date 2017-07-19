using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    public interface IRissoleEntity<T>
    {
        // properties
        IDbConnection Connection { get; set; }


        // functions
        IRissoleCommand<T> Build(string script, T model);

        IRissoleCommand<T> Build(IDbCommand command);

        IRissoleCommand<T> Select(T model);

        IRissoleCommand<T> Select(object primary);

        IRissoleCommand<T> Select(Func<T, bool> prdicate);

        IRissoleCommand<T> Single(T model);

        IRissoleCommand<T> Single(object primary);

        IRissoleCommand<T> Update(T model);

        IRissoleCommand<T> Update(IList<T> model);

        IRissoleCommand<T> Delete(T model);

        IRissoleCommand<T> Delete(object primary);

        IRissoleCommand<T> Delete(Func<T, bool> prdicate);
    }
}
