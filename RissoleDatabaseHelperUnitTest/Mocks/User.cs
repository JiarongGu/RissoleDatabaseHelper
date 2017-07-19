using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelperUnitTest.Mocks
{
    public class User
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string Surnname { get; set; }

        public string Phone { get; set; }

        public string Country { get; set; }

        public string State { get; set; }
    }
}
