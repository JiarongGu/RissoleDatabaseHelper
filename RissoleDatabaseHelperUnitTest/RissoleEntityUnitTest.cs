using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RissoleDatabaseHelper;
using RissoleDatabaseHelperUnitTest.Mocks;
using System.Data;
using MySql.Data.MySqlClient;

namespace RissoleDatabaseHelperUnitTest
{
    [TestClass]
    public class RissoleEntityUnitTest
    {
        MySqlConnection _connection;

        [TestInitialize]
        public void Initalize()
        {
            _connection = new MySqlConnection();
        }

        [TestMethod]
        public void TestCommandBuildNoExeception1()
        {
            IRissoleEntity<Account> rissoleEntity = new RissoleEntity<Account>(_connection);

            var command = rissoleEntity.Select(x => x).Join<User>((x, y) => x.AccountId == y.UserId)
                .Where(x => x.AccountId == Guid.NewGuid()).Custom("ORDER BY 1 DESC");
        }

        [TestMethod]
        public void TestCommandBuildNoExeception2()
        {
            IRissoleEntity<Account> rissoleEntity = new RissoleEntity<Account>(_connection);

            var command = rissoleEntity.Select(x => x).Where(x => x.Email.Contains("@info.com"));
        }

        [TestMethod]
        public void TestSelectionScriptBuild()
        {
            IRissoleEntity<Account> rissoleEntity = new RissoleEntity<Account>(_connection);
            var id = Guid.NewGuid();
            var command = rissoleEntity.Select(x => x).Where(x => x.AccountId == id);
            Assert.AreEqual(command.Parameters.Count, 1);
        }
    }
}
