using System.Transactions;
using Common;
using Common.Utilities;
using Logic.Domain;
using Logic.DomainObjects;
using Logic.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class UsersTest
    {
        private TransactionScope _transactionScope;

        [TestInitialize]
        public void Initialize()
        {
            _transactionScope = new TransactionScope();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _transactionScope.Dispose();
        }

        #region login functionality 

        [TestMethod]
        public void UserLogin_1()
        {
            var userDomain = new UsersDomain();
            var result = userDomain.AuthenticateUser("testAcc", "Pa$$w0rd!");
            Assert.IsTrue(result.ProcessResult == ProcessResult.Success);
        }

        [TestMethod]
        public void UserLogin_2()
        {
            var userDomain = new UsersDomain();
            var result = userDomain.AuthenticateUser("testAcc", "wrongPassword");
            Assert.IsFalse(result.ProcessResult == ProcessResult.Success);
        }


        [TestMethod]
        public void UserLogin_3()
        {
            var userDomain = new UsersDomain();
            var result = userDomain.AuthenticateUser(null, null);
            Assert.IsFalse(result.ProcessResult == ProcessResult.Success);
        }

        #endregion

        #region register functionality 

        [TestMethod]
        public void RegisterUser_1()
        {
            var userDomain = new UsersDomain();
            var paramsKeys = new Encryption().GeneratePublicAndPrivateKey();
            var user = new User
            {
                Username = "TestingMan",
                Email = "testing@hotmail.com",
                LoginTries = 0,
                Password = "Pa$$w0rd!",
                Actived = true,
                DateCreated = HomelessMethods.GetCurrentTime(),
                Locked = false,
                PrivateKey = paramsKeys.PrivateKey,
                PublicKey = paramsKeys.PublicKey,
                Version = 1,
                RoleName = "Admin"
            };

            var result = userDomain.Add(user);
            Assert.IsTrue(result.ProcessResult == ProcessResult.Success);
        }

        [TestMethod]
        public void RegisterUser_2()
        {
            var userDomain = new UsersDomain();
            var paramsKeys = new Encryption().GeneratePublicAndPrivateKey();
            var user = new User
            {
                Username = null, // ERROR HERE 
                Email = "testing@hotmail.com",
                LoginTries = 0,
                Password = "Pa$$w0rd!",
                Actived = true,
                DateCreated = HomelessMethods.GetCurrentTime(),
                Locked = false,
                PrivateKey = paramsKeys.PrivateKey,
                PublicKey = paramsKeys.PublicKey,
                Version = 1,
                RoleName = "Admin"
            };

            var result = userDomain.Add(user);
            Assert.IsTrue(result.ProcessResult == ProcessResult.Failure);
        }

        [TestMethod]
        public void RegisterUser_3()
        {
            var userDomain = new UsersDomain();
            var paramsKeys = new Encryption().GeneratePublicAndPrivateKey();
            var user = new User
            {
                Username = "testAcc2", // EXISTS 
                Email = "dylan.vassa22llo@hotmail.com", // EXISTS 
                LoginTries = 0,
                Password = "Pa$$w0rd!",
                Actived = true,
                DateCreated = HomelessMethods.GetCurrentTime(),
                Locked = false,
                PrivateKey = paramsKeys.PrivateKey,
                PublicKey = paramsKeys.PublicKey,
                Version = 1,
                RoleName = "Admin"
            };

            var result = userDomain.Add(user);
            Assert.IsTrue(result.ProcessResult == ProcessResult.Failure);
        }

        #endregion
    }
}
