using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RissoleDatabaseHelper;
using RissoleDatabaseHelperUnitTest.Mocks;

namespace RissoleDatabaseHelperUnitTest
{
    [TestClass]
    public class RissoleConditionBuilderUnitTest
    {
        RissoleConditionBuilder _rissoleConditionBuiler;

        [TestInitialize]
        public void Initalize()
        {
            _rissoleConditionBuiler = new RissoleConditionBuilder(RissoleProvider.Instance);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var account = new Account();
            var command = _rissoleConditionBuiler.ToSql<Account>(x => x.AccountId != account.AccountId);
            
            
        }
    }
}
