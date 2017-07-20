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
        private IRissoleProvider _rissoleProvider;
        private IRissoleCommand<T> _parentCommand; 

        private IDbCommand _command;
        private ICollection<IDbDataParameter> _parameters;

        private string _script;

        public string Script { get => _script; set => _script = value; }
        public ICollection<IDbDataParameter> Parameters { get => _parameters; }
        public IDbConnection Connection { get => _dbConnection; set => _dbConnection = value; }

        internal RissoleCommand(IDbConnection dbConnection, IRissoleProvider rissoleProvider)
        {
            _dbConnection = dbConnection;
            _rissoleProvider = rissoleProvider;
        }

        public int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            return await Task.Run(() => ExecuteNonQuery());
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _dbConnection.Close();
        }

        public IRissoleCommand<T> Where(Func<T, bool> prdicate)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> First(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> First(Func<T, bool> prdicate)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Join<TJoin>(Func<T, TJoin, bool> prdicate)
        {
            throw new NotImplementedException();
        }

        public T First()
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public List<T> ToList()
        {
            var executor = _rissoleProvider.GetRissoleExecutor<T>(_dbConnection);
            return executor.ExecuteReader(Command());
        }

        public IDbCommand Command()
        {
            if (_command == null)
            {
                _command = _dbConnection.CreateCommand();
                _command.CommandText = _script;

            }

            return _command;
        }
    }
}
