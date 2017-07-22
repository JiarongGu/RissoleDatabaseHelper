using RissoleDatabaseHelper.Core.Models;
using RissoleDatabaseHelper.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RissoleDatabaseHelper.Core
{
    public class RissoleExecutor<T> : IRissoleExecutor<T>, IDisposable
    {
        private readonly IRissoleCommand<T> _rissoleCommand;
        private readonly RissoleTable _table;

        internal RissoleExecutor(IRissoleCommand<T> rissoleCommand)
        {
            _rissoleCommand = rissoleCommand;
            _table = RissoleProvider.Instance.GetRissoleTable<T>();
        }

        /// <summary>
        /// Execute Read by Command Read list of Current Type
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public List<T> ExecuteReader(IDbCommand command)
        {
            command.Connection = _rissoleCommand.Connection;
            List<T> models = new List<T>();

            try
            {
                command.Connection.Open();
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
                command.Connection.Close();
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
            command.Connection = _rissoleCommand.Connection;
            int result;

            try
            {
                command.Connection.Open();
                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteNonQueryAsync -> Execute Failed", ex);
            }
            finally
            {
                command.Dispose();
                command.Connection.Close();
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
            command.Connection = _rissoleCommand.Connection;
            object result;

            try
            {
                command.Connection.Open();
                result = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteScalarAsync -> Execute Failed", ex);
            }
            finally
            {
                command.Dispose();
                command.Connection.Close();
            }

            return result;
        }

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

        /// <summary>
        /// Insert and get the new object in database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public T Insert(IDbCommand command, T model)
        {
            foreach (var computedData in _table.Columns.Where(x => x.IsComputed))
            {
                computedData.Property.SetValue(model, CreateComputedValue(computedData.DataType));
            }

            command = SetParameters(command, model, _table);

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

        private object CreateComputedValue(Type dataType)
        {
            switch (dataType.Name)
            {
                case "Guid": return Guid.NewGuid();
                default: throw new Exception("Unknow Computed Type: " + dataType.Name);
            }
        }

        private IDbCommand SetParameters(IDbCommand command, T model, RissoleTable table)
        {
            foreach (var column in table.Columns)
            {
                var propertyParameter = "@" + column.Property.Name;
                var propertyRegex = @"[^\w]" + propertyParameter + @"([^\w]|$)";

                if (Regex.Matches(command.CommandText, propertyRegex).Count > 0)
                {
                    var property = column.Property;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = propertyParameter;
                    parameter.Value = property.GetValue(model) == null ? DBNull.Value : property.GetValue(model);
                    parameter.DbType = RissoleData.TypeMap[property.GetType()];

                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
