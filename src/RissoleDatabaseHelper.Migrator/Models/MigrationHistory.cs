using RissoleDatabaseHelper.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Migrator.Models
{

    [Table("_migrationhistory")]
    public class MigrationHistory
    {
        [Column("MigrationhistoryId")]
        [PrimaryKey]
        public string MigrationhistoryId { get; set; }

        [Column("MigrationContextName")]
        public string MigrationContextName { get; set; }
    }
}
