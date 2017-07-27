using RissoleDatabaseHelper.Core.Commands;
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

        public IRissoleBaseCommand<T> Delete(Expression<Func<T, bool>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetDeleteScript<T>();

            return rissoleCommand.Where(prdicate);
        }

        public IRissoleBaseCommand<T> Delete(T model)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetDeleteScript<T>();

            return rissoleCommand.Where(model);
        }
        
        public IRissoleBaseCommand<T> Update(T model, bool includePirmaryKey = false)
        {
            var rissoleCommand = BuildUpdateRissoleCommand(model, includePirmaryKey, 0);
            return rissoleCommand;
        }

        public IRissoleBaseCommand<T> Update(IList<T> models, bool includePirmaryKey = false)
        {
            var rissoleCommands = new List<IRissoleBaseCommand<T>>();

            var stack = 0;
            foreach (var model in models)
            {
                var rissoleCommand = BuildUpdateRissoleCommand(model, includePirmaryKey, stack);

                stack = rissoleCommand.Stack;
                rissoleCommands.Add(rissoleCommand);
            }
            
            return ConcatRissoleCommands(rissoleCommands);
        }
        
        public IRissoleInsertCommand<T> Insert(T model)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetInsertScript<T>();
            rissoleCommand = rissoleCommand.InsertValues(model);

            return (IRissoleInsertCommand<T>)rissoleCommand.InsertValues(model);
        }

        public IRissoleBaseCommand<T> Select(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetSelectScript(prdicate).Script;

            return rissoleCommand;
        }

        public IRissoleBaseCommand<T> First(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            rissoleCommand.Script = _rissoleProvider.GetSelectScript(prdicate).Script;

            return rissoleCommand;
        }

        public IRissoleBaseCommand<T> Custom(string script)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script);
        }

        public IRissoleBaseCommand<T> Custom(string script, List<IDbDataParameter> parameters)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script, parameters);
        }

        public IRissoleBaseCommand<T> Custom(string script, params IDbDataParameter[] parameters)
        {
            return new RissoleCommand<T>(_dbConnection, _rissoleProvider, script, parameters.ToList());
        }

        public IDbConnection Connection {
            get { return _dbConnection; }
            set { _dbConnection = value; }
        }
        
        private IRissoleBaseCommand<T> BuildUpdateRissoleCommand(T model, bool includePirmaryKey, int stack)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider, stack);
            rissoleCommand.Script = _rissoleProvider.GetUpdateScript<T>();

            rissoleCommand = rissoleCommand.SetValues(model, false);

            return rissoleCommand.Where(model);
        }

        private IRissoleBaseCommand<T> ConcatRissoleCommands(List<IRissoleBaseCommand<T>> rissoleCommands)
        {
            var combinedScript = string.Join(" ", rissoleCommands.Select(x => x.Script + ";").ToList());
            var combinedParameters = new List<IDbDataParameter>();
            foreach (var rissoleCommand in rissoleCommands)
            {
                combinedParameters.AddRange(rissoleCommand.Parameters);
            }

            var combinedCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider, combinedScript, combinedParameters);

            return combinedCommand;
        }
    }
}
