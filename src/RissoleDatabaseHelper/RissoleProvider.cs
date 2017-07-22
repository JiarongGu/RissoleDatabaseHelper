using System;
using System.Collections.Generic;
using System.Text;
using RissoleDatabaseHelper.Core.Models;
using System.Linq.Expressions;
using System.Data;

namespace RissoleDatabaseHelper.Core
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

        public RissoleScript GetWhereCondition<T>(Expression<Func<T, bool>> expression)
        {
            var outTable = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { outTable };

            var rissoleScript = GetCondition(expression, rissoleTables);
            rissoleScript.Script = $"WHERE {rissoleScript.Script}";

            return rissoleScript;
        }
        
        public RissoleScript GetJoinScript<T, TJoin>(Expression<Func<T, TJoin, bool>> expression)
        {
            var joinTable = GetRissoleTable<TJoin>();
            var outTable = GetRissoleTable<T>();

            var rissoleTables = new List<RissoleTable>() { joinTable, outTable };

            var rissoleScript = GetCondition(expression, rissoleTables);
            rissoleScript.Script = $"JOIN {joinTable.Name} ON {rissoleScript.Script}";

            return rissoleScript;
        }

        public RissoleScript GetSelectCondition<T>(Expression<Func<T, object>> expression)
        {
            var outTable = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { outTable };

            var rissoleScript = GetCondition(expression, rissoleTables);
            rissoleScript.Script = $"SELECT {rissoleScript.Script} FROM {outTable.Name}";

            return rissoleScript;
        }
        
        private RissoleScript GetCondition(LambdaExpression expression, List<RissoleTable> rissoleTables)
        {
            return _rissoleConditionBuilder.RissoleScript(expression, rissoleTables);
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
