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
            ApiToken = Actions.Login(Config.Username, Config.Password);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginPasswordHeaders()
        {
            // Username, password, headers
            ApiToken = Actions.Login(Config.Username, Config.Password, true);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginSecureKeyNoHeaders()
        {
            // Username, secure key, no headers
            ApiToken = Actions.Login(Config.Username, SecureKey);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLoginSecureKeyHeaders()
        {
            // Username, secure key, headers
            ApiToken = Actions.Login(Config.Username, SecureKey, true);
            Assert.AreEqual(32, ApiToken.ToString("N").Length);
        }

        [TestMethod]
        public void TestLogout()
        {
            // Logout
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;
        }

        [TestMethod]
        public void TestGenerateApiKey()
        {
            SecureKey = Actions.GenerateApiKey(ApiToken, Config.Username);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
        }

        [TestMethod]
        public void TestGenerateApiKeyForceNew()
        {
            SecureKey = Actions.GenerateApiKey(ApiToken, Config.Username, true);
            Assert.AreEqual(32, SecureKey.ToString("N").Length);
            Actions.RemoveApiKey(ApiToken, SecureKey);
        }

        [TestMethod]
        public void TestRemoveApiKey()
        {
            Actions.RemoveApiKey(ApiToken, SecureKey);
            SecureKey = Guid.Empty;
        }

        [TestMethod]
        public void TestUpdatePassword()
        {
            Actions.UpdatePassword(ApiToken, Config.NewPassword);
            Actions.UpdatePassword(ApiToken, Config.Password);
        }
    }
}
