using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core.Commands
{
    public interface IRissoleBaseCommand<T>
    {
        string Script { get; set; }

        List<IDbDataParameter> Parameters { get; set; }

        IDbConnection Connection { get; set; }
        
        int Stack { get; }

        IRissoleBaseCommand<T> Where(Expression<Func<T, bool>> prdicate);

        IRissoleBaseCommand<T> Where(T model);

        IRissoleBaseCommand<T> Join<TJoin> (Expression<Func<T, TJoin, bool>> prdicate);

        IRissoleBaseCommand<T> Join<TFrom, TJoin>(Expression<Func<TFrom, TJoin, bool>> prdicate);

        IRissoleBaseCommand<T> Custom(string script, List<IDbDataParameter> parameters);

        IRissoleBaseCommand<T> Custom(string script, params IDbDataParameter[] parameters);
    }
}
