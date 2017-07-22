using RissoleDatabaseHelper.Core.Enums;
using RissoleDatabaseHelper.Core.Internals;
using RissoleDatabaseHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    /// <summary>
    /// Mapping class for MySql Connection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RissoleCommandExecutor<T>
    {
        private readonly IDbConnection _connection;
        private readonly RissoleTable _table;

        private readonly RissoleCommandBuilder _commandBuilder;
        private readonly RissoleDefinitionBuilder _definitionBuilder;

        private Dictionary<string, string> _scriptDictionary;

        public RissoleCommandExecutor(DbConnection connection)
        {
            _connection = connection;

            _commandBuilder = new RissoleCommandBuilder(connection);
            _definitionBuilder = new RissoleDefinitionBuilder();

            _scriptDictionary = new Dictionary<string, string>();

            _table = _definitionBuilder.BuildRissoleTable(typeof(T));
        }

        #region Basic Executions
        /// <summary>
        /// Execute Read by Command Read list of Current Type
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public List<T> ExecuteReader(IDbCommand command)
        {
            command.Connection = _connection;
            List<T> models = new List<T>();

            try
            {
                _connection.Open();
                IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    models.Add(CreateModelFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteReaderAsync -> Read Failed", ex);
            }
            finally
            {
                command.Dispose();
                _connection.Close();
            }

            return models;
        }

        /// <summary>
        /// Execute Non Query return number of row changes
        /// </summary>
        /// <param name="command"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IDbCommand command)
        {
            command.Connection = _connection;
            int result;

            try
            {
                _connection.Open();
                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteNonQueryAsync -> Execute Failed", ex);
            }
            finally
            {
                command.Dispose();
                _connection.Close();
            }

            return result;
        }
        
        /// <summary>
        /// Excute Query with a return value
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public object ExecuteScalar(IDbCommand command)
        {
            command.Connection = _connection;
            object result;

            try
            {
                _connection.Open();
                result = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteScalarAsync -> Execute Failed", ex);
            }
            finally
            {
                command.Dispose();
                _connection.Close();
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Insert and get the new object in database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Insert(T model)
        {
            foreach (var computedData in _table.Columns.Where(x => x.IsComputed))
            {
                computedData.Property.SetValue(model, CreateComputedValue(computedData.DataType));
            }

            var scriptName = GetScriptName("InsertSingleT_");
            var script = GetScriptFromDictionary(scriptName);

            if (script == null)
            {
                script = _commandBuilder.BuildInsertSingle(_table);
                _scriptDictionary.Add(scriptName, script);
            }

            var command = _commandBuilder.CreateCommand(script, model, _table);

            //insert into database
            var generatedId = (int)ExecuteScalar(command);

            //update model key
            var columnDefinition = _table.Columns.Where(x => x.IsGenerated == true).FirstOrDefault();

            if (columnDefinition != null)
            {
                columnDefinition.Property.SetValue(model, generatedId);
            }

            return model;
        }

        /// <summary>
        /// get the model with its given primary key details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Select(T model)
        {
            var scriptName = GetScriptName("SelectSingleT_");
            var script = GetScriptFromDictionary(scriptName);

            if (script == null)
            {
                script = _commandBuilder.BuildSelectSingle(_table);
                _scriptDictionary.Add(scriptName, script);
            }

            var command = _commandBuilder.CreateCommand(script, model, _table);

            return ExecuteReader(command).FirstOrDefault();
        }

        /// <summary>
        /// Fetch with model's primary key (auto assign if defined in attribute)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Select(object key)
        {
            var columnDefinition = _table.Columns.Where(x => x.Keys.Exists(y => y.Type == KeyType.PrimaryKey)).FirstOrDefault();

            if (columnDefinition == null) return default(T); //no primary key found

            T model = (T)Activator.CreateInstance(typeof(T));

            columnDefinition.Property.SetValue(model, key);

            return Select(model);
        }
        
        public bool Update(T model)
        {
            var scriptName = GetScriptName("UpdateSingleT_");
            var script = GetScriptFromDictionary(scriptName);

            if (script == null)
            {
                script = _commandBuilder.BuildUpdateSingle(_table);
                _scriptDictionary.Add(scriptName, script);
            }

            var command = _commandBuilder.CreateCommand(script, model, _table);

            return ExecuteNonQuery(command) > 0;
        }
        
        public bool Delete(T model)
        {
            var scriptName = GetScriptName("DeleteSingleT_");
            var script = GetScriptFromDictionary(scriptName);

            if (script == null)
            {
                script = _commandBuilder.BuildDeleteSingle(_table);
                _scriptDictionary.Add(scriptName, script);
            }

            var command = _commandBuilder.CreateCommand(script, model, _table);

            return ExecuteNonQuery(command) > 0;
        }

        public bool Delete(object key)
        {
            var columnDefinitions = _table.Columns.Where(x => x.Keys.Exists(y => y.Type == KeyType.PrimaryKey)).ToList();
            
            if (columnDefinitions.Count != 1) return false; //no primary key or multiple primary key found

            var columnDefinition = columnDefinitions.First();

            T model = (T)Activator.CreateInstance(typeof(T));

            columnDefinition.Property.SetValue(model, key);

            return Delete(model);
        }
        
        private object CreateComputedValue(Type dataType)
        {
            switch (dataType.Name)
            {
                case "Guid": return Guid.NewGuid();
                default: throw new Exception("Unknow Computed Type: " + dataType.Name);
            }
        }

        /// <summary>
        /// Get the reader data and put them into an object
        /// </summary>
        /// <param name="reader">sql reader</param>
        /// <returns>An object with reader data</returns>
        private T CreateModelFromReader(IDataReader reader)
        {
            T model = (T)Activator.CreateInstance(typeof(T));

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                var columnDefinition = _table.Columns.Where(x => x.Name == fieldName).FirstOrDefault();

                if (columnDefinition == null)
                    throw new Exception(String.Format("sql read field {0}, does not found in model map", fieldName));

                var property = columnDefinition.Property;

                //check if property is nullable type
                bool isNullableType = property.PropertyType.GetTypeInfo().IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

                //check if property can contain null value
                bool isNullable = isNullableType || property.PropertyType.GetTypeInfo().IsValueType == false;

                //get property underlying type
                string propertyTypeName = isNullableType ? property.PropertyType.GetGenericArguments()[0].GetTypeInfo().UnderlyingSystemType.Name : property.PropertyType.Name;

                //check if data column return null
                bool isDataNull = reader.IsDBNull(i);

                if (isDataNull)
                {
                    if (isNullable)
                    {
                        property.SetValue(model, null);
                    }
                    else
                    {
                        property.SetValue(model, Activator.CreateInstance(property.PropertyType));
                    }
                }
                else
                {
                    switch (propertyTypeName)
                    {
                        case "Guid": property.SetValue(model, reader.GetGuid(i)); break;
                        case "String": property.SetValue(model, reader.GetString(i)); break;
                        case "Int32": property.SetValue(model, reader.GetInt32(i)); break;
                        case "DateTime": property.SetValue(model, reader.GetDateTime(i)); break;
                        case "Decimal": property.SetValue(model, reader.GetDecimal(i)); break;
                        case "Boolean": property.SetValue(model, reader.GetBoolean(i)); break;
                        default: throw new Exception("Unknow Type: " + property.PropertyType.Name);
                    }
                }
            }
            return model;
        }
        
        private string GetScriptName(params object[] args)
        {
            var scriptName = new StringBuilder();

            foreach (var arg in args)
            {
                scriptName.Append(string.Format("{0}.{1}|", arg.GetType().Name, arg.ToString()));
            }
            scriptName.Length--;

            return scriptName.ToString();
        }

        private string GetScriptFromDictionary(string scriptName)
        {
            if (_scriptDictionary.ContainsKey(scriptName))
                return _scriptDictionary[scriptName];
            return null;
        }
    }
}
