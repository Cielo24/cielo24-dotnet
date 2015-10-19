using System;
using Cielo24;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace UnitTest
{
    [TestClass]
    public class ActionsTest
    {
        protected Actions actions = new Actions();
        protected Config config = new Config();
        protected Guid apiToken = Guid.Empty;
        protected Guid secureKey = Guid.Empty;
        // Storing memoryTarget here, so that unittests can access it
        protected MemoryTarget memoryTarget;

        private void EnableCustomLogger()
        {
            // Make WebUtils log to Console as well as to Memory
            var logConf = new LoggingConfiguration();
            memoryTarget = new MemoryTarget();
            var consoleTarget = new ConsoleTarget();

            logConf.AddTarget("MemoryTarget", memoryTarget);
            logConf.AddTarget("ConsoleTarget", consoleTarget);
            logConf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, memoryTarget));
            logConf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));

            LogManager.Configuration = logConf;
            WebUtils.logger = LogManager.GetLogger("TestLogger");
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            EnableCustomLogger();
            actions.ServerUrl = config.serverUrl;
            if (apiToken.Equals(Guid.Empty))
            {
                apiToken = actions.Login(config.username, config.password, true);
            }
            if (secureKey.Equals(Guid.Empty))
            {
                secureKey = actions.GenerateAPIKey(apiToken, config.username, true);
            }
        }
    }
}