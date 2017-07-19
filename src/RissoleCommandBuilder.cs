using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace RissoleDatabaseHelper
{
    internal class RissoleCommandBuilder
    {
        private IDbConnection _dbConnection;

        public RissoleCommandBuilder(IDbConnection dbConnection)
        {
            this._dbConnection = dbConnection;
        }
        
        internal string BuildSelectManyToMany<TFrom>(TFrom model, RissoleTable from, RissoleTable bind, RissoleTable output)
        {
            return $"{BuildSelectAll(output)} {BuildJoin(output, bind)} {BuildJoin(bind, from)} WHERE {BuildWhereConditionPrimaryKey(from)}";
        }

        internal string BuildSelectSingle(RissoleTable table)
        {
            StringBuilder whereCondition = new StringBuilder();
            StringBuilder mappedNames = new StringBuilder();

            foreach (var column in table.Columns)
            {
                if (column.IsPrimaryKey == true)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(BuildEqualsCondition(column));
                }

                if (mappedNames.Length > 0) mappedNames.Append(", ");
                mappedNames.Append(string.Format("`{0}`", column.Name));
            }

            string fetchScript = $"SELECT TOP(1) {mappedNames.ToString()} FROM {table.Name} WHERE {whereCondition.ToString()}";

            return fetchScript;
        }

        internal string BuildInsertSingle(RissoleTable table)
        {
            StringBuilder mappedNames = new StringBuilder();
            StringBuilder parameterNames = new StringBuilder();

            foreach (var column in table.Columns)
            {
                if (column.IsGenerated == false)
                {
                    if (mappedNames.Length > 0) mappedNames.Append(", ");
                    mappedNames.Append(String.Format("`{0}`", column.Name));

                    if (parameterNames.Length > 0) mappedNames.Append(", ");
                    parameterNames.Append(String.Format("@{0}", column.Property.Name));
                }
            }

            string insertScript = $"INSERT INTO {table.Name} ({mappedNames.ToString()}) VALUES ({parameterNames.ToString()}); SELECT LAST_INSERT_ID()";

            return insertScript;
        }

        internal string BuildUpdateSingle(RissoleTable table)
        {
            List<RissoleColumn> primaryKeyData = table.Columns.Where(x => x.IsPrimaryKey).ToList();
            List<RissoleColumn> modifiableData = table.Columns.Where(x => !x.IsGenerated).ToList();

            string whereCondition = BuildAppendedEqualsCodition(primaryKeyData, " AND ");
            string updateCondition = BuildAppendedEqualsCodition(modifiableData, ", ");

            return $"UPDATE {table.Name} Set {updateCondition} WHERE {whereCondition}";
        }

        internal string BuildDeleteSingle(RissoleTable table)
        {
            List<RissoleColumn> primaryKeyData = table.Columns.Where(x => x.IsPrimaryKey).ToList();
            string whereCondition = BuildAppendedEqualsCodition(primaryKeyData, " AND ");

            return $"DELETE FROM {table.Name} WHERE {whereCondition}";
        }
        
        private string BuildSelectAll(RissoleTable table)
        {
            return $"SELECT {BuildSelectAllColumn(table)} FROM {table.Name}";
        }

        private string BuildJoin(RissoleTable from, RissoleTable on)
        {
            List<RissoleColumn> foreignKeyColumns = on.Columns.Where(x => x.Keys != null && x.Keys.Count(y => y.TableName == from.Name) > 0).ToList();

            RissoleTable primaryTable = from;
            RissoleTable foreignTable = on;

            if (foreignKeyColumns.Count == 0)
            {
                foreignKeyColumns = from.Columns.Where(x => x.Keys != null
                    && x.Keys.Count(y => y.TableName == on.Name) > 0).ToList();
                primaryTable = on;
                foreignTable = from;
            }
            
            var joinConditions = new StringBuilder();
            foreach (var foreignKeyColumn in foreignKeyColumns)
            {
                List<RissoleKey> foreignKeys = foreignKeyColumn.Keys.Where(x => x.TableName == primaryTable.Name).ToList();

                if (foreignKeys.Count == 0)
                    continue;

                foreach(var foreignKey in foreignKeys)
                {
                    if (joinConditions.Length > 0)
                        joinConditions.Append(" AND ");

                    joinConditions.Append($"{foreignTable.Name}.{foreignKey.ColumnName} = {primaryTable.Name}.{foreignKey.ColumnName}");
                }
            }

            var script = $"JOIN {on.Name} ON {joinConditions.ToString()}";
            return script;
        }

        private string BuildSelectAllColumn(RissoleTable table)
        {
            var selectedColumns = new StringBuilder();
            foreach (var column in table.Columns)
            {
                if (selectedColumns.Length > 0)
                    selectedColumns.Append(", ");

                selectedColumns.Append($"{table.Name}.{column.Name}");
            }
            return selectedColumns.ToString();
        }
        
        private string BuildWhereConditionPrimaryKey(RissoleTable table)
        {
            var primaryKeys = table.Columns.Where(x => x.IsPrimaryKey).ToList();
            var whereConditions = new StringBuilder();

            foreach (var primaryKey in primaryKeys)
            {
                if (whereConditions.Length > 0)
                    whereConditions.Append(" AND ");

                whereConditions.Append(BuildEqualsCondition(primaryKey));
            }

            return whereConditions.ToString();
        }

        private string BuildEqualsCondition(RissoleColumn column)
        {
            return $"{column.Name} = @{column.Property.Name}";
        }

        private string BuildAppendedEqualsCodition(List<RissoleColumn> columns, string separator)
        {
            StringBuilder appendedCondition = new StringBuilder();
            foreach (var column in columns)
            {
                if (appendedCondition.Length > 0)
                    appendedCondition.Append(separator);

                appendedCondition.Append(BuildEqualsCondition(column));
            }
            return appendedCondition.ToString();
        }

        /// <summary>
        /// Insert the parameters into sql command based on the object passed in
        /// </summary>
        /// <param name="command">sql command</param>
        /// <param name="value">an object</param>
        /// <returns>sql command with parameter</returns>
        internal IDbCommand CreateCommand<TFrom>(string script, TFrom model, RissoleTable from)
        {
            var command = _dbConnection.CreateCommand();
            command.CommandText = script;
            command = SetCommandParameters<TFrom>(command, model,  from);
            return command;
        }

        internal IDbCommand SetCommandParameters<TFrom>(IDbCommand command, TFrom model, RissoleTable from)
        {
            foreach (var column in from.Columns)
            {
                var propertyParameter = "@" + column.Property.Name;
                var propertyRegex = @"[^\w]" + propertyParameter + @"([^\w]|$)";

                if (Regex.Matches(command.CommandText, propertyRegex).Count > 0)
                {
                    var property = column.Property;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = propertyParameter;
                    parameter.Value = property.GetValue(model) == null ? DBNull.Value : property.GetValue(model);

                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }
    }
}