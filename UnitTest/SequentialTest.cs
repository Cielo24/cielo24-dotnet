using System;
using Cielo24;
using Cielo24.JSON.Job;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class SequentialTest : ActionsTest
    {
        protected Guid jobId = Guid.Empty;

        [TestInitialize]
        public override void Initialize()
        {
            actions.ServerUrl = config.serverUrl;
            // Do nothing - we want to be able to control when we login/logout etc.
        }

        [TestMethod]
        public void testSequence()
        {
            // Login, generate API key, logout
            apiToken = actions.Login(config.username, config.password);
            secureKey = actions.GenerateAPIKey(apiToken, config.username, true);
            actions.Logout(apiToken);
            apiToken = Guid.Empty;

            // Login using API key
            apiToken = actions.Login(config.username, secureKey);

            // Create a job using a media URL
            jobId = actions.CreateJob(apiToken, ".NET_test_job").JobId;
            actions.AddMediaToJob(apiToken, jobId, config.sampleVideoUri);

            // Assert JobList and JobInfo data
            JobList list = actions.GetJobList(apiToken);
            Assert.IsTrue(containsJob(jobId, list), "JobId not found in JobList");
            Job job = actions.GetJobInfo(apiToken, jobId);
            Assert.AreEqual(jobId, job.JobId, "Wrong JobId found in JobInfo");

            // Logout
            actions.Logout(apiToken);
            apiToken = Guid.Empty;

            // Login/logout/change password 
            apiToken = actions.Login(config.username, config.password);
            actions.UpdatePassword(apiToken, config.newPassword);
            actions.Logout(apiToken);
            apiToken = Guid.Empty;

            // Change password back
            apiToken = actions.Login(config.username, config.newPassword);
            actions.UpdatePassword(apiToken, config.password);
            actions.Logout(apiToken);
            apiToken = Guid.Empty;

            // Login using API key
            apiToken = actions.Login(config.username, secureKey);

            // Delete job and assert JobList data
            actions.DeleteJob(apiToken, jobId);
            JobList list2 = actions.GetJobList(apiToken);
            Assert.IsFalse(containsJob(jobId, list2), "JobId should not be in JobList");

            // Delete current API key and try to re-login (should fail)
            actions.RemoveAPIKey(apiToken, secureKey);
            actions.Logout(apiToken);
            apiToken = Guid.Empty;

            try
            {
                apiToken = actions.Login(config.username, secureKey);
                Assert.Fail("Should not be able to login using invalid API key");
            }
            catch (EnumWebException e)
            {
                Assert.AreEqual(ErrorType.ACCOUNT_UNPRIVILEGED.ToString(), e.ErrorType, "Unexpected error type");
            }
        }

        private bool containsJob(Guid jobId, JobList list)
        {
            foreach(Job j in list.ActiveJobs)
            {
                if(j.JobId.Equals(jobId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
