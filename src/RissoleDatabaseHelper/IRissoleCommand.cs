using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    public interface IRissoleCommand<T>
    {
        int ExecuteNonQuery();

        object ExecuteScalar();

        List<T> ExecuteReader();

        string Script { get; set; }

        List<IDbDataParameter> Parameters { get; set; }

        IDbConnection Connection { get; set; }
        
        int Stack { get; }

        IRissoleCommand<T> Where(Expression<Func<T, bool>> prdicate);

        IRissoleCommand<T> Where(T model);

        IRissoleCommand<T> Join<TJoin> (Expression<Func<T, TJoin, bool>> prdicate);
        
        IRissoleCommand<T> SetValues(T model, bool ignorePrimaryKey);

        IRissoleCommand<T> Custom(string script, List<IDbDataParameter> parameters);

        IRissoleCommand<T> Custom(string script, params IDbDataParameter[] parameters);

        IDbCommand BuildCommand();

        T First();

        T FirstOrDefault();

        List<T> ToList();
    }
}
