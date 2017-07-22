using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
            var rissoleScript = _rissoleProvider.GetDeleteScript<T>();
            rissoleCommand.Script = rissoleScript.Script;

            return rissoleCommand.Where(prdicate);
        }

        public IRissoleCommand<T> Delete(T model)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            var rissoleScript = _rissoleProvider.GetDeleteScript<T>();
            rissoleCommand.Script = rissoleScript.Script;



            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Delete(object primary)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            var rissoleScript = _rissoleProvider.GetDeleteScript<T>();
            rissoleCommand.Script = rissoleScript.Script;

            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Update(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Update(IList<T> model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Select(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            var rissoleScript = _rissoleProvider.GetSelectScript(prdicate);

            rissoleCommand.Script = rissoleScript.Script;

            return rissoleCommand;
        }

        public IRissoleCommand<T> First(Expression<Func<T, object>> prdicate)
        {
            var rissoleCommand = new RissoleCommand<T>(_dbConnection, _rissoleProvider);
            var rissoleScript = _rissoleProvider.GetSelectScript(prdicate);

            rissoleCommand.Script = rissoleScript.Script;

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

        public IRissoleCommand<T> Custom(string script, T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Custom(string script, List<IDbDataParameter> parameters)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Custom(string script, params IDbDataParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public IDbConnection Connection {
            get { return _dbConnection; }
            set { _dbConnection = value; }
        }
    }
}
