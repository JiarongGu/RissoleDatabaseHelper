using System;
using System.Collections.Generic;
using System.Text;
using RissoleDatabaseHelper.Core.Models;
using System.Linq.Expressions;
using System.Data;
using System.Linq;
using RissoleDatabaseHelper.Core.Enums;

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

        public RissoleScript GetSelectScript<T>(Expression<Func<T, object>> expression)
        {
            var table = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { table };

            var rissoleScript = GetCondition(expression, rissoleTables);
            rissoleScript.Script = $"SELECT {rissoleScript.Script} FROM {table.Name}";

            return rissoleScript;
        }
        
        private RissoleScript GetCondition(LambdaExpression expression, List<RissoleTable> rissoleTables, int stack = 0)
        {
            return _rissoleConditionBuilder.RissoleScript(expression, rissoleTables, stack);
        }

        public RissoleScript GetFirstScript<T>(Expression<Func<T, object>> expression)
        {
            var table = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { table };

            var rissoleScript = GetCondition(expression, rissoleTables);
            rissoleScript.Script = $"SELECT TOP(1) {rissoleScript.Script} FROM {table.Name}";

            return rissoleScript;
        }

        public RissoleScript GetDeleteScript<T>()
        {
            var table = GetRissoleTable<T>();
            var script = $"DELETE FROM {table.Name}";

            return new RissoleScript(script);
        }
        
        public RissoleScript GetWhereScript<T>(Expression<Func<T, bool>> expression, int stack)
        {
            var table = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { table };

            var rissoleScript = GetCondition(expression, rissoleTables, stack);
            rissoleScript.Script = $"WHERE {rissoleScript.Script}";

            return rissoleScript;
        }

        public RissoleScript GetJoinScript<T, TJoin>(Expression<Func<T, TJoin, bool>> expression, int stack)
        {
            var joinTable = GetRissoleTable<TJoin>();
            var outTable = GetRissoleTable<T>();

            var rissoleTables = new List<RissoleTable>() { joinTable, outTable };

            var rissoleScript = GetCondition(expression, rissoleTables, stack);
            rissoleScript.Script = $"JOIN {joinTable.Name} ON {rissoleScript.Script}";

            return rissoleScript;
        }

        public RissoleScript GetPrimaryScript<T>(T model, int stack)
        {
            var table = GetRissoleTable<T>();
            var primary = table.Columns.Where(x => x.Keys.Exists(y => y.Type == KeyType.PrimaryKey)).ToList();
            

            throw new NotImplementedException();
        }

        public RissoleScript GetPrimaryScript<T>(Dictionary<string, object> keys, int stack)
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
