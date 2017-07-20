using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RissoleDatabaseHelper;
using RissoleDatabaseHelperUnitTest.Mocks;
using System.Data;

namespace RissoleDatabaseHelperUnitTest
{
    [TestClass]
    public class RissoleConditionBuilderUnitTest
    {
        [TestInitialize]
        public void Initalize()
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            var account = new Account();
            var user = new User();

            IRissoleEntity<Account> rissoleEntity = new RissoleEntity<Account>(null);

            var command = rissoleEntity.Select(x => x).Join<User>((x, y) => x.AccountId == y.UserId)
                .Where(x => x.AccountId == Guid.NewGuid()).Custom("ORDER BY 1 DESC");

        }
    }
}
