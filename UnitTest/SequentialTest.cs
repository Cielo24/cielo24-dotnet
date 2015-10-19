using System;
using System.Linq;
using Cielo24;
using Cielo24.JSON.Job;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Test_for_cielo24.NET_library
{
    [TestClass]
    public class SequentialTest : ActionsTest
    {
        protected Guid JobId = Guid.Empty;

        [TestInitialize]
        public override void Initialize()
        {
            Actions.ServerUrl = Config.ServerUrl;
            // Do nothing - we want to be able to control when we login/logout etc.
        }

        [TestMethod]
        public void TestSequence()
        {
            // Login, generate API key, logout
            ApiToken = Actions.Login(Config.Username, Config.Password);
            SecureKey = Actions.GenerateAPIKey(ApiToken, Config.Username, true);
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login using API key
            ApiToken = Actions.Login(Config.Username, SecureKey);

            // Create a job using a media URL
            JobId = Actions.CreateJob(ApiToken, ".NET_test_job").JobId;
            Actions.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);

            // Assert JobList and JobInfo data
            var list = Actions.GetJobList(ApiToken);
            Assert.IsTrue(ContainsJob(JobId, list), "JobId not found in JobList");
            var job = Actions.GetJobInfo(ApiToken, JobId);
            Assert.AreEqual(JobId, job.JobId, "Wrong JobId found in JobInfo");

            // Logout
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login/logout/change password 
            ApiToken = Actions.Login(Config.Username, Config.Password);
            Actions.UpdatePassword(ApiToken, Config.NewPassword);
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Change password back
            ApiToken = Actions.Login(Config.Username, Config.NewPassword);
            Actions.UpdatePassword(ApiToken, Config.Password);
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login using API key
            ApiToken = Actions.Login(Config.Username, SecureKey);

            // Delete job and assert JobList data
            Actions.DeleteJob(ApiToken, JobId);
            var list2 = Actions.GetJobList(ApiToken);
            Assert.IsFalse(ContainsJob(JobId, list2), "JobId should not be in JobList");

            // Delete current API key and try to re-login (should fail)
            Actions.RemoveAPIKey(ApiToken, SecureKey);
            Actions.Logout(ApiToken);
            ApiToken = Guid.Empty;

            try
            {
                ApiToken = Actions.Login(Config.Username, SecureKey);
                Assert.Fail("Should not be able to login using invalid API key");
            }
            catch (EnumWebException e)
            {
                Assert.AreEqual(ErrorType.ACCOUNT_UNPRIVILEGED.ToString(), e.ErrorType, "Unexpected error type");
            }
        }

        private static bool ContainsJob(Guid jobId, JobList list)
        {
            return list.ActiveJobs.Any(j => j.JobId.Equals(jobId));
        }
    }
}
