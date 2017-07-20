using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    public interface IRissoleCommand<T>
    {
        int ExecuteNonQuery();

        object ExecuteScalar();

        string Script { get; set; }

        ICollection<IDbDataParameter> Parameters { get; }

        IDbConnection Connection { get; set; }
        
        IRissoleCommand<T> Where(Func<T, bool> prdicate);

        IRissoleCommand<T> First(T model);

        IRissoleCommand<T> First(Func<T, bool> prdicate);

        IRissoleCommand<T> Join<TJoin> (Func<T, TJoin, bool> prdicate);

        T First();

        T FirstOrDefault();

        List<T> ToList();
    }
}
