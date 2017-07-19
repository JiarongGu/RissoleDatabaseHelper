using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RissoleDatabaseHelper
{
    public class RissoleEntity<T> : IRissoleEntity<T>
    {
        private IDbConnection _dbConnection;
        public IDbConnection Connection { get => _dbConnection; set => _dbConnection = value; }

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

        public IRissoleCommand<T> Delete(Func<T, bool> prdicate)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Select(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Select(object primary)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Select(Func<T, bool> prdicate)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Single(T model)
        {
            throw new NotImplementedException();
        }

        public IRissoleCommand<T> Single(object primary)
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
    }
}
