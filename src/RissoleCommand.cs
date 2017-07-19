using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace RissoleDatabaseHelper
{
    public class RissoleCommand<T> : IRissoleCommand<T>, IDisposable
    {
        private IDbConnection _dbConnection;
        private ICollection<IDbDataParameter> _parameters;
        private string _script;

        public string Script { get => _script; set => _script = value; }
        public ICollection<IDbDataParameter> Parameters { get => _parameters; }
        public IDbConnection Connection { get => _dbConnection; set => _dbConnection = value; }
        
        public int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            return await Task.Run(() => ExecuteNonQuery());
        }
        
        public List<T> ExecuteReader()
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            _dbConnection.Dispose();
        }
    }
}
