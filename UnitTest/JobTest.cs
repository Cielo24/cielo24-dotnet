using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cielo24;
using Cielo24.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unit_Test_for_cielo24.NET_library
{
    [TestClass]
    public class JobTest : ActionsTest
    {
        protected Guid JobId = Guid.Empty;
        protected Guid TaskId = Guid.Empty;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            // Always start with a fresh job
            JobId = Actions.CreateJob(ApiToken).JobId;
        }

        [TestMethod]
        public void TestOptions()
        {
            var options = new CaptionOptions
            {
                CaptionBySentence = true,
                ForceCase = Case.UPPER
            };
            string[] array = { "build_url=true", "dfxp_header=header" };
            options.PopulateFromArray(array);
            Assert.AreEqual("build_url=true&caption_by_sentence=true&dfxp_header=header&force_case=upper", options.ToQuery().ToLower());
        }

        [TestMethod]
        public void TestCreateJob()
        {
            var result = Actions.CreateJob(ApiToken, "test_name", Language.ENGLISH);
            Assert.AreEqual(32, result.JobId.ToString("N").Length);
            Assert.AreEqual(32, result.TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAuthorizeJob()
        {
            Actions.AuthorizeJob(ApiToken, JobId);
        }

        [TestMethod]
        public void TestDeleteJob()
        {
            TaskId = Actions.DeleteJob(ApiToken, JobId);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestGetJobInfo()
        {
            Actions.GetJobInfo(ApiToken, JobId);
        }

        [TestMethod]
        public void TestGetJobList()
        {
            Actions.GetJobList(ApiToken);
        }

        [TestMethod]
        public void TestGetElementList()
        {
            Actions.GetElementList(ApiToken, JobId);
        }

        [TestMethod]
        public void TestGetListOfElementLists()
        {
            Actions.GetListOfElementLists(ApiToken, JobId);
        }

        [TestMethod]
        public void TestGetMedia()
        {
            // Add media to job first
            Actions.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);
            // Test get media
            Actions.GetMedia(ApiToken, JobId);
        }

        [TestMethod]
        public void TestGetTranscript()
        {
            Actions.GetTranscript(ApiToken, JobId);
        }

        [TestMethod]
        public void TestGetCaption()
        {
            Actions.GetCaption(ApiToken, JobId, CaptionFormat.SRT);
        }

        [TestMethod]
        public void TestGetCaptionBuildUrl()
        {
            var options = new CaptionOptions(buildUri:true);
            var response = Actions.GetCaption(ApiToken, JobId, CaptionFormat.SRT, options);
            new Uri(response);
        }

        [TestMethod]
        public void TestPerformTranscription()
        {
            Actions.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);
            var callbackUri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            TaskId = Actions.PerformTranscription(ApiToken, JobId, Fidelity.PREMIUM, Priority.STANDARD, callbackUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestPerformTranscriptionCallbackUrlEncoding()
        {
            var callbackUri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            const string encodedUri = "callback_url=http:%2F%2Ffake-callback.com%2Faction%3Fapi_token%3D1234%26job_id%3D{job_id}";
            Actions.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);
            TaskId = Actions.PerformTranscription(ApiToken, JobId, Fidelity.PREMIUM, Priority.STANDARD, callbackUri);
            // Last log entry will contain the callback to perform_transcription
            Assert.IsTrue(MemoryTarget.Logs.Last().Contains(encodedUri));
        }

        [TestMethod]
        public void TestAddMediaToJobUrl()
        {
            TaskId = Actions.AddMediaToJob(ApiToken, JobId, Config.SampleVideoUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAddMediaToJobEmbedded()
        {
            TaskId = Actions.AddEmbeddedMediaToJob(ApiToken, JobId, Config.SampleVideoUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAddMediaToJobFile()
        {
            var fs = new FileStream(Config.SampleVideoFilePath, FileMode.Open);
            TaskId = Actions.AddMediaToJob(ApiToken, JobId, fs);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void testAggregateStatistics()
        {
            var metrics = new List<string> {"billable_minutes_total", "billable_minutes_professional"};
            var result = Actions.AggregateStatistics(ApiToken, metrics, "month", new DateTime(2015, 6, 25), new DateTime(2015, 7, 25), "*");
            Assert.IsNotNull(result);
        }
    }
}
