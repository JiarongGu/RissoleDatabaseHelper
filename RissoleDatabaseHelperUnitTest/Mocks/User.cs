using RissoleDatabaseHelper.Attributes;
using RissoleDatabaseHelper.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelperUnitTest.Mocks
{
    [Table("users")]
    public class User
    {
        [Column("user_id")]
        [Key(KeyType.PrimaryKey, IsComputed = true)]
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
