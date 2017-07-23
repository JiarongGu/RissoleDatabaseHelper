using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    public class RissoleEntity<T> : IRissoleEntity<T>
    {
        private IDbConnection _dbConnection;
        private IRissoleProvider _rissoleProvider;
        private RissoleTable _rissoleTable;

        public RissoleEntity(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _rissoleProvider = RissoleProvider.Instance;
            _rissoleTable = _rissoleProvider.GetRissoleTable<T>();
        }

        public IRissoleCommand<T> Delete(Expression<Func<T, bool>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetDeleteScript<T>().Script;

            return rissoleCommand.Where(prdicate);
        }

        public IRissoleCommand<T> Delete(T model)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetDeleteScript<T>().Script;

            return rissoleCommand.Where(model);
        }
        
        public IRissoleCommand<T> Update(T model, bool includePirmaryKey = false)
        {
            IRissoleCommand<T> rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetUpdateScript<T>().Script;

            rissoleCommand = rissoleCommand.SetValues(model, false);
            rissoleCommand = rissoleCommand.Where(model);

            return rissoleCommand;
        }

        public IRissoleCommand<T> Update(IList<T> models, bool includePirmaryKey = false)
        {
            var rissoleCommands = new List<IRissoleCommand<T>>();
            var stack = 0;
            foreach (var model in models)
            {
                IRissoleCommand<T> rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider, stack);
                rissoleCommand.Script = _rissoleProvider.GetUpdateScript<T>().Script;
                rissoleCommand = rissoleCommand.SetValues(model, false);
                rissoleCommand = rissoleCommand.Where(model);

                stack = rissoleCommand.Stack;
                rissoleCommands.Add(rissoleCommand);
            }

            var combinedScript = string.Join(" ", rissoleCommands.Select(x => x.Script + ";").ToList());
            var combinedParameters = new List<IDbDataParameter>();
            foreach (var rissoleCommand in rissoleCommands)
            {
                combinedParameters.AddRange(rissoleCommand.Parameters);
            }

            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, combinedScript, combinedParameters);
        }

        public IRissoleCommand<T> Select(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetSelectScript(prdicate).Script;

            return rissoleCommand;
        }

        public IRissoleCommand<T> First(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetSelectScript(prdicate).Script;

            return rissoleCommand;
        }

        public IRissoleCommand<T> Insert(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Insert(List<T> model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Custom(string script)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script);
        }

        public IRissoleCommand<T> Custom(string script, List<IDbDataParameter> parameters)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script, parameters);
        }

        public IRissoleCommand<T> Custom(string script, params IDbDataParameter[] parameters)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script, parameters.ToList());
        }

        public IDbConnection Connection {
            get { return _dbConnection; }
            set { _dbConnection = value; }
        }
    }
}
