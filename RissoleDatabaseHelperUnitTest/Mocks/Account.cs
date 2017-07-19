using RissoleDatabaseHelper.Attributes;
using RissoleDatabaseHelper.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelperUnitTest.Mocks
{
    [Table("accounts")]
    public class Account
    {
        [Column(Name = "account_id", IsComputed = true, DataType = typeof(Guid))]
        [Key(KeyType.PrimaryKey, IsComputed = true)]
        public Guid AccountId { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("security_stamp")]
        public string SecurityStamp { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("lockout_end_date_utc")]
        public DateTime? LockoutEndDateUtc { get; set; }

        [Column("lockout_enabled")]
        public bool LockoutEnabled { get; set; }

        [Column("access_failed_count")]
        public int AccessFailedCount { get; set; }
    }
}
