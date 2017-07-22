using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RissoleDatabaseHelper.Core;
using RissoleDatabaseHelperTests.Core.Mocks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace RissoleDatabaseHelperTests.Core
{
    [TestClass]
    public class RissoleEntityTests
    {
        MySqlConnection _connection;
        IRissoleEntity<Account> _accounts;
        IRissoleEntity<User> _users;

        [TestInitialize]
        public void Initalize()
        {
            _connection = new MySqlConnection();
            _accounts = new RissoleEntity<Account>(_connection);
            _users = new RissoleEntity<User>(_connection);
        }

        [TestMethod]
        public void TestCommandBuildNoExeception1()
        {

            var command = _accounts.Select(x => x).Join<User>((x, y) => x.AccountId == y.UserId)
                .Where(x => x.AccountId == Guid.NewGuid()).Custom("ORDER BY 1 DESC");
        }

        [TestMethod]
        public void TestCommandBuildNoExeception2()
        {
            var command = _accounts.Select(x => x).Where(x => x.Email.Contains("@info.com"));
        }

        [TestMethod]
        public void TestCommandBuildIsNull()
        {
            var command = _accounts.Select(x => x).Where(x => x.Email == null);
            Assert.AreEqual(command.Parameters.Count, 1);
            Assert.IsTrue(command.Script.Contains("IS @NULL"));
        }

        [TestMethod]
        public void TestCommandBuildIsNotNull()
        {
            var command = _accounts.Select(x => x).Where(x => x.Email != null);
            Assert.AreEqual(command.Parameters.Count, 1);
            Assert.IsTrue(command.Script.Contains("IS NOT @NULL"));
        }

        [TestMethod]
        public void TestCommandBuildDelete1()
        {
            var account = new Account();
            account.AccountId = Guid.NewGuid();
            var command = _accounts.Delete(account);

            Assert.AreEqual(command.Parameters.Count, 1);
        }

        [TestMethod]
        public void TestSelectionScriptBuild()
        {
            var id = Guid.NewGuid();
            var command = _accounts.Select(x => new { x.Username, x.AccountId }).Where(x => x.AccountId == id);
            Assert.AreEqual(command.Parameters.Count, 1);
        }

        [TestMethod]
        public void TestScriptConditionContains()
        {
            var ids = new List<Guid>() ;
            ids.Add(Guid.NewGuid());
            ids.Add(Guid.NewGuid());
            ids.Add(Guid.NewGuid());

            var command = _accounts.Select(x => x).Where(x => ids.Contains(x.AccountId));
            Assert.AreEqual(command.Parameters.Count, 3);
        }
    }
}
