using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
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
        public void testLoginPasswordNoHeaders()
        {
            // Username, password, no headers
            apiToken = actions.Login(config.username, config.password, false);
            Assert.AreEqual(32, apiToken.ToString("N").Length);
        }

        [TestMethod]
        public void testLoginPasswordHeaders()
        {
            // Username, password, headers
            apiToken = actions.Login(config.username, config.password, true);
            Assert.AreEqual(32, apiToken.ToString("N").Length);
        }

        [TestMethod]
        public void testLoginSecureKeyNoHeaders()
        {
            // Username, secure key, no headers
            apiToken = actions.Login(config.username, secureKey, false);
            Assert.AreEqual(32, apiToken.ToString("N").Length);
        }

        [TestMethod]
        public void testLoginSecureKeyHeaders()
        {
            // Username, secure key, headers
            apiToken = actions.Login(config.username, secureKey, true);
            Assert.AreEqual(32, apiToken.ToString("N").Length);
        }

        [TestMethod]
        public void testLogout()
        {
            // Logout
            actions.Logout(apiToken);
            apiToken = Guid.Empty;
        }

        [TestMethod]
        public void testGenerateApiKey()
        {
            secureKey = actions.GenerateAPIKey(apiToken, config.username, false);
            Assert.AreEqual(32, secureKey.ToString("N").Length);
        }

        [TestMethod]
        public void testGenerateApiKeyForceNew()
        {
            secureKey = actions.GenerateAPIKey(apiToken, config.username, true);
            Assert.AreEqual(32, secureKey.ToString("N").Length);
            actions.RemoveAPIKey(apiToken, secureKey);
        }

        [TestMethod]
        public void testRemoveApiKey()
        {
            actions.RemoveAPIKey(apiToken, secureKey);
            secureKey = Guid.Empty;
        }

        [TestMethod]
        public void testUpdatePassword()
        {
            actions.UpdatePassword(apiToken, config.newPassword);
            actions.UpdatePassword(apiToken, config.password);
        }
    }
}
