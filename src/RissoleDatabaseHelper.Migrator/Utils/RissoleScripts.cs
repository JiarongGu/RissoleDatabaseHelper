using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Migrator.Utils
{
    public static class RissoleScripts
    {
        public const string CREATE_MIGRATIONHISTORY_DATABASE = @"CREATE TABLE IF NOT EXISTS _migrationhistory (MigrationhistoryId NVARCHAR(150) NOT NULL, MigrationContextName NVARCHAR(150) NULL, PRIMARY KEY (MigrationhistoryId));";
    }
}
