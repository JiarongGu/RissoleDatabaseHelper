using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RissoleDatabaseHelper.Core
{
    public class RissoleCommand<T> : IRissoleCommand<T>, IDisposable
    {
        private IDbConnection _dbConnection;
        private IRissoleProvider _rissoleProvider;
        private IRissoleCommand<T> _parentCommand; 

        private IDbCommand _command;
        private List<IDbDataParameter> _parameters;

        private string _script;
        private int _stack;

        public string Script
        {
            get { return _script; }
            set { _script = value; }
        }

        internal RissoleCommand(IDbConnection dbConnection, IRissoleProvider rissoleProvider, string script, List<IDbDataParameter> parameters)
            : this(dbConnection, rissoleProvider, script)
        {
            _parameters = parameters;
        }

        internal RissoleCommand(IDbConnection dbConnection, IRissoleProvider rissoleProvider, string script)
            :this(dbConnection, rissoleProvider)
        {
            _script = script;
        }

        internal RissoleCommand(IDbConnection dbConnection, IRissoleProvider rissoleProvider)
        {
            _dbConnection = dbConnection;
            _rissoleProvider = rissoleProvider;
            _parameters = new List<IDbDataParameter>();
            _stack = 0;
        }

        internal RissoleCommand(RissoleCommand<T> rissoleCommand)
        {
            _parentCommand = rissoleCommand;
            _rissoleProvider = rissoleCommand._rissoleProvider;
            _script += rissoleCommand._script;
            _stack = rissoleCommand.Stack + 1;
        }
        
        public int Stack {
            get { return _stack; }
        }

        public List<IDbDataParameter> Parameters
        {
            get
            {
                if (_parentCommand == null)
                    return _parameters;
                return _parentCommand.Parameters;
            }

            set
            {
                if (_parentCommand == null)
                {
                    _parameters = value;
                }
                else
                {
                    _parentCommand.Parameters = value;
                }
            }
        }

        public IDbConnection Connection
        {
            get
            {
                if (_parentCommand == null)
                    return _dbConnection;
                return _parentCommand.Connection;
            }

            set
            {
                if (_parentCommand == null)
                {
                    _dbConnection = value;
                }
                else
                {
                    _parentCommand.Connection = value;
                }
            }
        }

        public IRissoleCommand<T> Join<TJoin>(Expression<Func<T, TJoin, bool>> prdicate)
        {
            var rissoleScript = _rissoleProvider.GetJoinScript(prdicate, Stack);

            var rissoleCommand = new RissoleCommand<T>(this);
            rissoleCommand.Script += " " + rissoleScript.Script;
            rissoleCommand.Parameters.AddRange(GetParameterFromRissoleScript(rissoleScript));

            return rissoleCommand;
        }

        public IRissoleCommand<T> Where(Expression<Func<T, bool>> prdicate)
        {
            var rissoleScript = _rissoleProvider.GetWhereScript(prdicate, Stack);

            var rissoleCommand = new RissoleCommand<T>(this);
            rissoleCommand.Script += " " + rissoleScript.Script;
            rissoleCommand.Parameters.AddRange(GetParameterFromRissoleScript(rissoleScript));

            return rissoleCommand;
        }

        public IRissoleCommand<T> Find(T model)
        {
            var rissoleScript = _rissoleProvider.GetPrimaryScript(model, Stack);

            var rissoleCommand = new RissoleCommand<T>(this);
            rissoleCommand.Script += " " + rissoleScript.Script;
            rissoleCommand.Parameters.AddRange(GetParameterFromRissoleScript(rissoleScript));

            return rissoleCommand;
        }

        public IRissoleCommand<T> Custom(string script, List<IDbDataParameter> parameters)
        {
            var rissoleCommand = new RissoleCommand<T>(this);
            rissoleCommand.Script += " " + script;
            rissoleCommand.Parameters.AddRange(parameters);
            
            return rissoleCommand;
        }

        private ICollection<IDbDataParameter> GetParameterFromRissoleScript(RissoleScript rissoleScript)
        {
            var tempCommand = Connection.CreateCommand();
            List<IDbDataParameter> parameters = new List<IDbDataParameter>();
            foreach (var scriptParam in rissoleScript.Parameters)
            {
                var parameter = tempCommand.CreateParameter();
                parameter.ParameterName = scriptParam.Key;

                if (scriptParam.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = scriptParam.Value;
                    parameter.DbType = RissoleQueryDictionary.TypeMap[scriptParam.Value.GetType()];
                }

                parameters.Add(parameter);
            }
            return parameters;
        }

        public IRissoleCommand<T> Custom(string script, params IDbDataParameter[] parameters)
        {
            return Custom(script, new List<IDbDataParameter>(parameters));
        }

        public IDbCommand BuildCommand()
        {
            if (_command == null)
            {
                _command = Connection.CreateCommand();
                _command.CommandText = _script;

                foreach (var parameter in Parameters)
                {
                    _command.Parameters.Add(parameter);
                }
            }

            return _command;
        }
        
        public T First()
        {
            var executor = new RissoleExecutor<T>(this);
            return executor.ExecuteReader(BuildCommand()).First();
        }

        public T FirstOrDefault()
        {
            var executor = new RissoleExecutor<T>(this);
            return executor.ExecuteReader(BuildCommand()).FirstOrDefault();
        }

        public List<T> ToList()
        {
            return ExecuteReader();
        }

        public List<T> ExecuteReader()
        {
            var executor = new RissoleExecutor<T>(this);
            return executor.ExecuteReader(BuildCommand());
        }
        
        public int ExecuteNonQuery()
        {
            var executor = new RissoleExecutor<T>(this);
            return executor.ExecuteNonQuery(BuildCommand());
        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            return await Task.Run(() => ExecuteNonQuery());
        }

        public object ExecuteScalar()
        {
            var executor = new RissoleExecutor<T>(this);
            return executor.ExecuteScalar(BuildCommand());
        }

        public void Dispose()
        {
            _dbConnection.Close();
        }
    }
}
