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
            ApiClient.ServerUrl = Config.ServerUrl;
            // Do nothing - we want to be able to control when we login/logout etc.
        }

        [TestMethod]
        public void TestSequence()
        {
            // Login, generate API key, logout
            ApiToken = ApiClient.Login(Config.Username, Config.Password);
            SecureKey = ApiClient.GenerateApiKey(ApiToken, Config.Username, true);
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login using API key
            ApiToken = ApiClient.Login(Config.Username, SecureKey);

            // Create a job using a media URL
            JobId = ApiClient.CreateJob(ApiToken, ".NET_test_job").JobId;
            ApiClient.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);

            // Assert JobList and JobInfo data
            var list = ApiClient.GetJobList(ApiToken);
            Assert.IsTrue(ContainsJob(JobId, list), "JobId not found in JobList");
            var job = ApiClient.GetJobInfo(ApiToken, JobId);
            Assert.AreEqual(JobId, job.JobId, "Wrong JobId found in JobInfo");

            // Logout
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login/logout/change password 
            ApiToken = ApiClient.Login(Config.Username, Config.Password);
            ApiClient.UpdatePassword(ApiToken, Config.NewPassword);
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Change password back
            ApiToken = ApiClient.Login(Config.Username, Config.NewPassword);
            ApiClient.UpdatePassword(ApiToken, Config.Password);
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;

            // Login using API key
            ApiToken = ApiClient.Login(Config.Username, SecureKey);

            // Delete job and assert JobList data
            ApiClient.DeleteJob(ApiToken, JobId);
            var list2 = ApiClient.GetJobList(ApiToken);
            Assert.IsFalse(ContainsJob(JobId, list2), "JobId should not be in JobList");

            // Delete current API key and try to re-login (should fail)
            ApiClient.RemoveApiKey(ApiToken, SecureKey);
            ApiClient.Logout(ApiToken);
            ApiToken = Guid.Empty;

            try
            {
                ApiToken = ApiClient.Login(Config.Username, SecureKey);
                Assert.Fail("Should not be able to login using invalid API key");
            }
            catch (EnumWebException e)
            {
                Assert.AreEqual(ErrorType.AccountUnprivileged.ToString(), e.ErrorType, "Unexpected error type");
            }
        }

        private static bool ContainsJob(Guid jobId, JobList list)
        {
            return list.ActiveJobs.Any(j => j.JobId.Equals(jobId));
        }
    }
}
