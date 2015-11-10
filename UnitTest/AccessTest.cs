using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cielo24;

namespace Unit_Test_for_cielo24.NET_library
{
    [TestClass]
    public class AccessTest : ActionsTest
    {
        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TestMethod]
        public void TestLoginPassword()
        {
            // Username, password, no headers
            var client = ApiClient.Login(Config.Username, Config.Password, Config.ServerUrl);
            Assert.AreEqual(32, client.ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginSecureKey()
        {
            // Username, secure key, no headers
            var client = ApiClient.Login(Config.Username, SecureKey, Config.ServerUrl);
            Assert.AreEqual(32, client.ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLogout()
        {
            // Logout
            ApiClient.Logout();
        }

        [TestMethod]
        public void TestGenerateApiKey()
        {
            SecureKey = ApiClient.GenerateApiKey(Config.Username);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
        }

        [TestMethod]
        public void TestGenerateApiKeyForceNew()
        {
            SecureKey = ApiClient.GenerateApiKey(Config.Username, true);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
            ApiClient.RemoveApiKey(SecureKey);
        }

        [TestMethod]
        public void TestRemoveApiKey()
        {
            ApiClient.RemoveApiKey(SecureKey);
            SecureKey = Guid.Empty;
        }

        [TestMethod]
        public void TestUpdatePassword()
        {
            ApiClient.UpdatePassword(Config.NewPassword);
            ApiClient.UpdatePassword(Config.Password);
        }
    }
}
