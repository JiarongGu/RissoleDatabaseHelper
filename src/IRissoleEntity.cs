﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
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
        
        IRissoleCommand<T> Delete(T model);

        IRissoleCommand<T> Delete(object primary);

        IRissoleCommand<T> Update(T model);

        IRissoleCommand<T> Update(IList<T> model);
        
        IRissoleCommand<T> Select(Expression<Func<T, object>> prdicate);

        IRissoleCommand<T> Select<TFrom>(Expression<Func<T, object>> prdicate);

        IRissoleCommand<T> Select(string script);
    }
}
