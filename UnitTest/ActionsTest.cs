using System;
using Cielo24;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Unit_Test_for_cielo24.NET_library
{
    [TestClass]
    public class ActionsTest
    {
        protected Actions Actions = new Actions();
        protected Config Config = new Config();
        protected Guid ApiToken = Guid.Empty;
        protected Guid SecureKey = Guid.Empty;
        // Storing memoryTarget here, so that unittests can access it
        protected MemoryTarget MemoryTarget;

        private void EnableCustomLogger()
        {
            // Make WebUtils log to Console as well as to Memory
            var logConf = new LoggingConfiguration();
            MemoryTarget = new MemoryTarget();
            var consoleTarget = new ConsoleTarget();

            logConf.AddTarget("MemoryTarget", MemoryTarget);
            logConf.AddTarget("ConsoleTarget", consoleTarget);
            logConf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, MemoryTarget));
            logConf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, consoleTarget));

            LogManager.Configuration = logConf;
            WebUtils.Logger = LogManager.GetLogger("TestLogger");
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            EnableCustomLogger();
            Actions.ServerUrl = Config.ServerUrl;
            if (ApiToken.Equals(Guid.Empty))
            {
                ApiToken = Actions.Login(Config.Username, Config.Password, true);
            }
            if (SecureKey.Equals(Guid.Empty))
            {
                SecureKey = Actions.GenerateApiKey(ApiToken, Config.Username, true);
            }
        }
    }
}