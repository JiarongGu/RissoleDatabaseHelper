﻿using RissoleDatabaseHelper.Core.Attributes;
using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelperTests.Core.Mocks
{
    [Table("accounts")]
    public class Account
    {
        [Column("account_id", IsComputed = true)]
        [PrimaryKey]
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
