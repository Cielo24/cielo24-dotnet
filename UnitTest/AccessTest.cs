using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void TestLoginPasswordNoHeaders()
        {
            // Username, password, no headers
            ApiToken = ApiClient.Login(Config.Username, Config.Password);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginPasswordHeaders()
        {
            // Username, password, headers
            ApiToken = ApiClient.Login(Config.Username, Config.Password, true);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginSecureKeyNoHeaders()
        {
            // Username, secure key, no headers
            ApiToken = ApiClient.Login(Config.Username, SecureKey);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginSecureKeyHeaders()
        {
            // Username, secure key, headers
            ApiToken = ApiClient.Login(Config.Username, SecureKey, true);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLogout()
        {
            // Logout
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;
        }

        [TestMethod]
        public void TestGenerateApiKey()
        {
            SecureKey = ApiClient.GenerateApiKey(ApiToken, Config.Username);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
        }

        [TestMethod]
        public void TestGenerateApiKeyForceNew()
        {
            SecureKey = ApiClient.GenerateApiKey(ApiToken, Config.Username, true);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
            ApiClient.RemoveApiKey(ApiToken, SecureKey);
        }

        [TestMethod]
        public void TestRemoveApiKey()
        {
            ApiClient.RemoveApiKey(ApiToken, SecureKey);
            SecureKey = Guid.Empty;
        }

        [TestMethod]
        public void TestUpdatePassword()
        {
            ApiClient.UpdatePassword(ApiToken, Config.NewPassword);
            ApiClient.UpdatePassword(ApiToken, Config.Password);
        }
    }
}
