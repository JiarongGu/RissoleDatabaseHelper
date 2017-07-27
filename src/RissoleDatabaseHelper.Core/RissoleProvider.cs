using System;
using System.Collections.Generic;
using System.Text;
using RissoleDatabaseHelper.Core.Models;
using System.Linq.Expressions;
using System.Data;
using System.Linq;
using RissoleDatabaseHelper.Core.Enums;
using RissoleDatabaseHelper.Core.Exceptions;

namespace RissoleDatabaseHelper.Core
{
    internal class RissoleProvider : IRissoleProvider
    {
        private static RissoleProvider _rissoleProvider;
        private RissoleDefinitionBuilder _rissoleDefinitionBuilder;
        private RissoleConditionBuilder _rissoleConditionBuilder;

        private Dictionary<Type, RissoleTable> _rissoleTables;
        private Dictionary<string, string> _rissoleScripts;
        private Dictionary<Tuple<IDbConnection, ReferencedScriptType>, RissoleReferencedScript> _referencedScripts;

        private RissoleProvider() {
            _rissoleTables = new Dictionary<Type, RissoleTable>();
            _rissoleDefinitionBuilder = new RissoleDefinitionBuilder();
            _rissoleConditionBuilder = new RissoleConditionBuilder();
            _referencedScripts = new Dictionary<Tuple<IDbConnection, ReferencedScriptType>, RissoleReferencedScript>();
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

        public string GetDeleteScript<T>()
        {
            var table = GetRissoleTable<T>();
            var script = $"DELETE FROM {table.Name}";

            return script;
        }
        
        public string GetInsertScript<T>()
        {
            var table = GetRissoleTable<T>();
            var script = $"INSERT INTO {table.Name}";

            return script;
        }

        public string GetUpdateScript<T>()
        {
            var table = GetRissoleTable<T>();
            var script = $"UPDATE {table.Name} SET";

            return script;
        }

        public RissoleScript GetWhereScript<T>(Expression<Func<T, bool>> expression, int stack)
        {
            var table = GetRissoleTable<T>();
            var rissoleTables = new List<RissoleTable>() { table };

            var rissoleScript = GetCondition(expression, rissoleTables, stack);
            rissoleScript.Script = $"WHERE {rissoleScript.Script}";

            return rissoleScript;
        }

        public RissoleScript GetWhereScript<T>(T model, int stack)
        {
            var table = GetRissoleTable<T>();
            var columns = table.Columns.Where(x => x.Keys.Exists(y => y.Type == KeyType.PrimaryKey)).ToList();

            if (columns.Count == 0) throw new RissoleException($"Table {table.Name} has no primary key defined.");

            var parameters = GetRissoleParameters(table, columns, model, stack);

            var script = $"WHERE ({string.Join(" AND ", parameters.Select(x => $"{x.ColumnName} = {x.ParameterName}"))})";

            return new RissoleScript(script, parameters);
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

        public RissoleScript GetInsertValueScript<T>(T model, int stack)
        {
            var table = GetRissoleTable<T>();
            var columns = table.Columns.Where(x => !x.IsGenerated).ToList();

            var parameters = GetRissoleInsertParameters(table, columns, model, stack);

            var script = $"{string.Join(", ", parameters.Select(x => x.ColumnName))} VALUES ({string.Join(", ", parameters.Select(x => x.ParameterName))})";

            return new RissoleScript(script, parameters);
        }

        public RissoleScript GetSetValueScript<T>(T model, int stack, bool includePirmaryKey)
        {
            var table = GetRissoleTable<T>();
            var columns = table.Columns.Where(x => 
                (includePirmaryKey || !x.Keys.Exists(y => y.Type == KeyType.PrimaryKey) 
                && !(x.IsComputed || x.IsGenerated))).ToList();
            var parameters = GetRissoleParameters(table, columns, model, stack);

            var script = string.Join(", ", parameters.Select(x => $"{x.ColumnName} = {x.ParameterName}"));

            return new RissoleScript(script, parameters);
        }
        
        /// <summary>
        /// Get database dependent script based on connection and command types
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="scriptType"></param>
        /// <returns></returns>
        public string GetConnectionScript(IDbConnection dbConnection, ReferencedScriptType scriptType)
        {
            var key = new Tuple<IDbConnection, ReferencedScriptType>(dbConnection, scriptType);

            if (!_referencedScripts.ContainsKey(key))
            {
                var referencedScripts = RissoleDictionary.GetReferenceScripts(scriptType);
                var vaildReferenceScript = GetVaildReferencedScript(dbConnection, referencedScripts);

                if(vaildReferenceScript == null)
                    throw new RissoleException($"No vaild command for {dbConnection.ConnectionString}, {scriptType.ToString()}");

                _referencedScripts.Add(key, vaildReferenceScript);
            }

            return _referencedScripts[key].ActualScript;
        }

        private RissoleReferencedScript GetVaildReferencedScript(IDbConnection dbConnection, List<RissoleReferencedScript> referencedScripts)
        {
            foreach (var script in referencedScripts)
            {
                if (IsScriptVaild(dbConnection, script.TrialScript))
                {
                    return script;
                }
            }
            return null;
        }
         
        private bool IsScriptVaild(IDbConnection dbConnection, string script)
        {
            bool result = false;

            dbConnection.Open();
            using (var transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.CommandText = script;
                        dbCommand.Transaction = transaction;
                        dbCommand.ExecuteNonQuery();
                    }

                    result = true;
                }
                catch
                {
                    result = false;
                }

                transaction.Rollback();
                transaction.Dispose();
            }

            dbConnection.Close();
            return result;
        }
        
        /// <summary>
        /// Get Parameter value and name by model and table definition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="model"></param>
        /// <param name="stack"></param>
        /// <returns>ColumnNames, Parameters</returns>
        private List<RissoleParameter> GetRissoleParameters<T>(RissoleTable table, List<RissoleColumn> columns, T model, int stack)
        {
            var parameters = new List<RissoleParameter>();
            
            foreach (var column in columns)
            {
                var columnName = $"{table.Name}.{column.Name}";
                var parameterName = $"@{columnName}_{stack}";
                var value = column.Property.GetValue(model);
                
                parameters.Add(new RissoleParameter(columnName, parameterName, value));
            }

            return parameters;
        }

        private List<RissoleParameter> GetRissoleInsertParameters<T>(RissoleTable table, List<RissoleColumn> columns, T model, int stack)
        {
            var parameters = new List<RissoleParameter>();

            foreach (var column in columns)
            {
                var columnName = $"{table.Name}.{column.Name}";
                var parameterName = $"@{columnName}_{stack}";
                var value = GetColumnPropertyValue(column, model);

                parameters.Add(new RissoleParameter(columnName, parameterName, value));
            }

            return parameters;
        }

        private object GetColumnPropertyValue<T>(RissoleColumn column, T model)
        {
            if (column.IsComputed)
                return CreateComputedValue(column.DataType);

            return column.Property.GetValue(model);
        }

        private object CreateComputedValue(Type dataType)
        {
            if (dataType == typeof(Guid)) return Guid.NewGuid();

            throw new Exception("Unknow Computed Type: " + dataType.Name);
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
