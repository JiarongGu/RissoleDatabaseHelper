using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    public interface IRissoleCommand<T>
    {
        List<T> ExecuteReader();

        int ExecuteNonQuery();

        object ExecuteScalar();

        string Script { get; set; }

        ICollection<IDbDataParameter> Parameters { get; }

        IDbConnection Connection { get; set; }
    }
}
