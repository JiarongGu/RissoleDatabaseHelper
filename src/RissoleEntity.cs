﻿using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace RissoleDatabaseHelper
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

        public IRissoleCommand<T> Build(string script, T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Build(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Delete(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Delete(object primary)
        {
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
            var rissoleScript = _rissoleProvider.GetSelectCondition(prdicate);

            rissoleCommand.Script = rissoleScript.Script;
            rissoleCommand.Parameters.AddRange(rissoleScript.Parameters);

            return rissoleCommand;
        }

        public IRissoleCommand<T> Select<TFrom>(Expression<Func<T, object>> prdicate)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Select(string script)
        {
            throw new NotImplementedException();
        }

        public IDbConnection Connection { get => _dbConnection; set => _dbConnection = value; }
    }
}
