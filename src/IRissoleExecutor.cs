using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    public interface IRissoleExecutor<T>
    {
        List<T> ExecuteReader(IDbCommand command);

        int ExecuteNonQuery(IDbCommand command);

        object ExecuteScalar(IDbCommand command);

        T Insert(IDbCommand command, T model);
    }
}
