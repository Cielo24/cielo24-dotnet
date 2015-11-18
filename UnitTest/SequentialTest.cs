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

        [TestMethod]
        public void TestSequence()
        {
            // Login, generate API key, logout
            var client = ApiClient.Login(Config.Username, Config.Password, Config.ServerUrl);
            var secureKey = client.GenerateApiKey(Config.Username, true);
            client.Logout();

            // Login using API key
            client = ApiClient.Login(Config.Username, secureKey, Config.ServerUrl);

            // Create a job using a media URL
            JobId = client.CreateJob( ".NET_test_job").JobId;
            client.AddMediaToJob(JobId, Config.SampleVideoUri);

            // Assert JobList and JobInfo data
            var list = client.GetJobList();
            Assert.IsTrue(ContainsJob(JobId, list), "JobId not found in JobList");
            var job = client.GetJobInfo(JobId);
            Assert.AreEqual(JobId, job.JobId, "Wrong JobId found in JobInfo");

            // Logout
            client.Logout();

            // Login/logout/change password 
            client = ApiClient.Login(Config.Username, Config.Password, Config.ServerUrl);
            client.UpdatePassword(Config.NewPassword);
            client.Logout();

            // Change password back
            client = ApiClient.Login(Config.Username, Config.NewPassword, Config.ServerUrl);
            client.UpdatePassword(Config.Password);
            client.Logout();

            // Login using API key
            client = ApiClient.Login(Config.Username, secureKey, Config.ServerUrl);

            // Delete job and assert JobList data
            client.DeleteJob(JobId);
            var list2 = ApiClient.GetJobList();
            Assert.IsFalse(ContainsJob(JobId, list2), "JobId should not be in JobList");

            // Delete current API key and try to re-login (should fail)
            client.RemoveApiKey(secureKey);
            client.Logout();

            try
            {
                ApiClient.Login(Config.Username, secureKey, Config.ServerUrl);
                Assert.Fail("Should not be able to login using invalid API key");
            }
            catch (EnumWebException e)
            {
                Assert.AreEqual("ACCOUNT_UNPRIVILEGED", e.ErrorType, "Unexpected error type");
            }
        }

        private static bool ContainsJob(Guid jobId, JobList list)
        {
            return list.ActiveJobs.Any(j => j.JobId.Equals(jobId));
        }
    }
}
