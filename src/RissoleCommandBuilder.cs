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