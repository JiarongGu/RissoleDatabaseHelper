using RissoleDatabaseHelper.Core.Commands;
using RissoleDatabaseHelper.Core.Enums;
using RissoleDatabaseHelper.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RissoleDatabaseHelper.Core
{
    public static class RissoleExecutor
    {
        /// <summary>
        /// Execute Read by Command Read list of Current Type
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static List<T> ExecuteReader<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            List<T> models = new List<T>();

            try
            {
                using (var command = BuildCommand(rissoleCommand))
                {
                    rissoleCommand.Connection.Open();
                    IDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        models.Add(CreateModelFromReader<T>(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteReader -> Read Failed", ex);
            }
            finally
            {
                rissoleCommand.Connection.Close();
            }

            return models;
        }

        /// <summary>
        /// Execute Non Query return number of row changes
        /// </summary>
        /// <param name="command"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            int result;

            try
            {
                using (var command = BuildCommand(rissoleCommand))
                {
                    rissoleCommand.Connection.Open();
                    result = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteNonQuery -> Execute Failed", ex);
            }
            finally
            {
                rissoleCommand.Connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Excute Query with a return value
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static object ExecuteScalar<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            object result;

            try
            {
                using (var command = BuildCommand(rissoleCommand))
                {
                    rissoleCommand.Connection.Open();
                    result = command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("MySqlExecutor.ExecuteScalar -> Execute Failed", ex);
            }
            finally
            {
                rissoleCommand.Connection.Close();
            }

            return result;
        }

        public static List<object> ExecuteScalar<T>(this List<IRissoleBaseCommand<T>> rissoleCommands)
        {
            List<object> results = new List<object>();

            if (rissoleCommands == null || rissoleCommands.Count == 0) return results;

            var connection = rissoleCommands.First().Connection;

            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                bool commit = true;

                foreach (var rissoleCommand in rissoleCommands)
                {
                    try
                    {
                        var command = rissoleCommand.BuildCommand();
                        var result = command.ExecuteScalar();
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        results.Add(ex);
                        commit = false;
                    }
                }

                if (commit == true)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }

                transaction.Dispose();

            }

            connection.Close();

            return results;
        }

        public static int Exec<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            return rissoleCommand.ExecuteNonQuery();
        }

        public static T Exec<T>(this IRissoleInsertCommand<T> rissoleCommand)
        {
            var rissoleProvider = RissoleProvider.Instance;
            var connection = rissoleCommand.Connection;

            var lastInsertScript = rissoleProvider.GetConnectionScript(connection, QueryCommandType.GetLastInsert);
            var lastInsertCommand = new RissoleCommand<T>(connection, rissoleProvider, lastInsertScript);
            
            List<IRissoleBaseCommand<T>> rissoleCommands = new List<IRissoleBaseCommand<T>>();
            rissoleCommands.Add(rissoleCommand);
            rissoleCommands.Add(lastInsertCommand);
            
            return rissoleCommand.ExecuteReader().FirstOrDefault();
        }

        public static T First<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            return rissoleCommand.ExecuteReader().First();
        }

        public static T FirstOrDefault<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            return rissoleCommand.ExecuteReader().FirstOrDefault();
        }

        public static List<T> ToList<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            return rissoleCommand.ExecuteReader();
        }

        public static IDbCommand BuildCommand<T>(this IRissoleBaseCommand<T> rissoleCommand)
        {
            var command = rissoleCommand.Connection.CreateCommand();
            command.CommandText = rissoleCommand.Script;

            foreach (var parameter in rissoleCommand.Parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        private static T CreateModelFromReader<T>(IDataReader reader)
        {
            var table = RissoleProvider.Instance.GetRissoleTable<T>();
            T model = (T)Activator.CreateInstance(typeof(T));

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldName = reader.GetName(i);
                var column = table.Columns.Where(x => x.Name == fieldName).FirstOrDefault();

                if (column == null)
                    throw new RissoleException($"sql read field {fieldName}, does not found in model map");

                var property = column.Property;
                var propertyType = property.PropertyType;
                
                //check if property can contain null value
                bool isNullable = Nullable.GetUnderlyingType(propertyType) != null || propertyType.GetTypeInfo().IsValueType == false;

                //check if data column return null
                bool isDataNull = reader.IsDBNull(i);

                //get database type
                var dataType = RissoleDictionary.DbTypeMap[propertyType];

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
                    continue;
                }
                
                //TODO: try reader.getValue();
                switch (dataType)
                {
                    case DbType.Byte:
                        property.SetValue(model, reader.GetByte(i));
                        break;
                    case DbType.Int16:
                        property.SetValue(model, reader.GetInt16(i));
                        break;
                    case DbType.Int32:
                        property.SetValue(model, reader.GetInt32(i));
                        break;
                    case DbType.Int64:
                        property.SetValue(model, reader.GetInt64(i));
                        break;
                    case DbType.Double:
                        property.SetValue(model, reader.GetDouble(i));
                        break;
                    case DbType.Decimal:
                        property.SetValue(model, reader.GetDecimal(i));
                        break;
                    case DbType.Boolean:
                        property.SetValue(model, reader.GetBoolean(i));
                        break;
                    case DbType.String:
                        property.SetValue(model, reader.GetString(i));
                        break;
                    case DbType.StringFixedLength:
                        property.SetValue(model, reader.GetChar(i));
                        break;
                    case DbType.Guid:
                        property.SetValue(model, reader.GetGuid(i));
                        break;
                    case DbType.DateTime:
                        property.SetValue(model, reader.GetDateTime(i));
                        break;
                    case DbType.DateTimeOffset:
                        property.SetValue(model, reader.GetDateTime(i));
                        break;
                    case DbType.SByte:
                        property.SetValue(model, Convert.ToSByte(reader.GetByte(i)));
                        break;
                    case DbType.UInt16:
                        property.SetValue(model, Convert.ToUInt16(reader.GetInt16(i)));
                        break;
                    case DbType.UInt32:
                        property.SetValue(model, Convert.ToUInt32(reader.GetInt32(i)));
                        break;
                    case DbType.UInt64:
                        property.SetValue(model, Convert.ToUInt64(reader.GetInt64(i)));
                        break;
                    case DbType.Single:
                        property.SetValue(model, Convert.ToSingle(reader.GetDouble(i)));
                        break;
                    case DbType.Binary:
                        property.SetValue(model, BitConverter.GetBytes(reader.GetDouble(i)));
                        break;
                    default: throw new Exception("Unknow Type: " + propertyType.Name);
                }
            }
            return model;
        }

    }
}
