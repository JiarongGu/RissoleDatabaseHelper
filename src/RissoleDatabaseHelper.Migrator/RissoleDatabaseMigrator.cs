using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;
using RissoleDatabaseHelper.Migrator.Utils;

namespace RissoleDatabaseHelper.Migrator
{
    public class RissoleDatabaseMigrator
    {
        private IDbConnection _connection;
        private Assembly _assembly;

        public RissoleDatabaseMigrator(IDbConnection connection, Assembly assembly)
        {
            _connection = connection;
            _assembly = assembly;
        }
        
        public void Setup()
        {
            SetupDatabase(_connection);
            SetupHistoryTable();
            Update();
        }

        private void Update()
        {
            Regex regex = new Regex(@"\.Scripts\..*\.sql");
            List<string> sqlResourceNames = _assembly.GetManifestResourceNames().Where(x => regex.IsMatch(x)).OrderBy(x => x).ToList();

            foreach (string sqlResourceName in sqlResourceNames)
            {
                TryUpdateSqlResource(sqlResourceName);
            }
        }

        private void TryUpdateSqlResource(string resourceName)
        {
            //check if the query already executed
            string fetchScript = $"SELECT COUNT(*) FROM _migrationhistory WHERE MigrationContextName = '{resourceName}'";

            bool foundExecuted = int.Parse(ExecuteScalar(fetchScript, _connection)) > 0;

            if (foundExecuted) return;

            string sqlResourceScript;
            
            using (Stream stream = _assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    sqlResourceScript = reader.ReadToEnd();
                }
            }

            if (!string.IsNullOrWhiteSpace(sqlResourceScript))
            {
                //execute embadded script
                ExecuteNonQuery(@sqlResourceScript, _connection);

                //insert history
                Regex regex = new Regex(@"\.Scripts\.(?<filename>.*\.sql)");
                Match match = regex.Matches(resourceName).Cast<Match>().FirstOrDefault();
                string filename = match.Groups["filename"].Value;

                string historyId = DateTime.Now.ToString("yyyyMMddHHmmssfff_") + filename;
                string insertHistory = $"INSERT INTO _migrationhistory VALUES('{historyId}','{resourceName}')";
                ExecuteNonQuery(insertHistory, _connection);
            }
        }

        /// <summary>
        /// initalize database exist if not create 
        /// </summary>
        private void SetupDatabase(IDbConnection dbConnection)
        {
            string database = dbConnection.Database;
            dbConnection.ChangeDatabase("");

            string script = $"{RissoleConstants.CREATE_DATABASE_IFNOTEXISTS} {database}";
            ExecuteNonQuery(script, dbConnection);
        }

        /// <summary>
        /// initalize table for database history tracking
        /// </summary>
        private void SetupHistoryTable()
        {
            string script = RissoleScripts.CREATE_MIGRATIONHISTORY_DATABASE;
            ExecuteNonQuery(script, _connection);
        }

        private void ExecuteNonQuery(string script, IDbConnection connection)
        {
            connection.Open();
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = script;
                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        private string ExecuteScalar(string script, IDbConnection connection)
        {
            string result;

            connection.Open();
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = script;
                result = command.ExecuteScalar().ToString();
            }
            connection.Close();

            return result;
        }
    }
}
