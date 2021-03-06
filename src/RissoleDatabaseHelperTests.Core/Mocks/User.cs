﻿using RissoleDatabaseHelper.Core.Attributes;
using RissoleDatabaseHelper.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelperTests.Core.Mocks
{
    [Table("users")]
    public class User
    {
        [Column("user_id", IsComputed = true)]
        [PrimaryKey]
        public Guid UserId { get; set; }

        [Column("firstname")]
        public string FirstName { get; set; }

        [Column("othername")]
        public string OtherName { get; set; }

        [Column("surnname")]
        public string Surnname { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("country")]
        public string Country { get; set; }

        [Column("state")]
        public string State { get; set; }
    }
}
