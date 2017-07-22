using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Core.Utils
{
    public static class RissoleConstants
    {
        // sql script constants
        public const string CREATE_DATABASE_IFNOTEXISTS = "CREATE DATABASE IF NOT EXISTS";

        // db migration
        public const string MIGRATIONHISTORY_TABLENAME = "_migrationhistory";
    }
}
