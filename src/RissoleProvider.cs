using System;
using System.Collections.Generic;
using System.Text;
using RissoleDatabaseHelper.Models;
using System.Linq.Expressions;
using System.Data;

namespace RissoleDatabaseHelper
{
    internal class RissoleProvider : IRissoleProvider
    {
        private static RissoleProvider _rissoleProvider;
        private RissoleDefinitionBuilder _rissoleDefinitionBuilder;
        private RissoleConditionBuilder _rissoleConditionBuilder;

        private Dictionary<Type, RissoleTable> _rissoleTables;
        
        private RissoleProvider() {
            _rissoleTables = new Dictionary<Type, RissoleTable>();
            _rissoleDefinitionBuilder = new RissoleDefinitionBuilder();
            _rissoleConditionBuilder = new RissoleConditionBuilder();
        }

        public RissoleTable GetRissoleTable<T>()
        {
            Type model = typeof(T);

            if (_rissoleTables.ContainsKey(model))
                return _rissoleTables[model];

            var rissoleTable = _rissoleDefinitionBuilder.BuildRissoleTable(model);
            _rissoleTables.Add(model, rissoleTable);

            return rissoleTable;
        }

        public string GetWhereCondition<T>(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public RissoleCommandExecutor<T> GetRissoleExecutor<T>()
        {
            throw new NotImplementedException();
        }

        public RissoleCommandExecutor<T> GetRissoleExecutor<T>(IDbConnection dbConnection)
        {
            throw new NotImplementedException();
        }

        public static IRissoleProvider Instance {
            get {
                if (_rissoleProvider == null)
                    _rissoleProvider = new RissoleProvider();
                return _rissoleProvider;
            }
        }
    }
}
