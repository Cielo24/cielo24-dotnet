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
            var secureKey = ApiClient.GenerateApiKey(Config.Username);
            var client = ApiClient.Login(Config.Username, secureKey, Config.ServerUrl);
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
            var secureKey = ApiClient.GenerateApiKey(Config.Username);
            Assert.AreEqual(32, secureKey.ToString("N").Length);
        }

        [TestMethod]
        public void TestGenerateApiKeyForceNew()
        {
            var secureKey = ApiClient.GenerateApiKey(Config.Username, true);
            Assert.AreEqual(32, secureKey.ToString("N").Length);
            ApiClient.RemoveApiKey(secureKey);
        }

        [TestMethod]
        public void TestRemoveApiKey()
        {
            var secureKey = ApiClient.GenerateApiKey(Config.Username);
            ApiClient.RemoveApiKey(secureKey);
        }

        [TestMethod]
        public void TestUpdatePassword()
        {
            ApiClient.UpdatePassword(Config.NewPassword);
            ApiClient.UpdatePassword(Config.Password);
        }
    }
}
