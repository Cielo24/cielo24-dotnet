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
            JobId = ApiClient.CreateJob().JobId;
        }

        [TestMethod]
        public void TestOptions()
        {
            var options = new CaptionOptions
            {
                CaptionBySentence = true,
                ForceCase = Case.Upper
            };
            string[] array = { "build_url=true", "dfxp_header=header" };
            options.PopulateFromArray(array);
            Assert.AreEqual("build_url=true&caption_by_sentence=true&dfxp_header=header&force_case=upper", options.ToQuery().ToLower());
        }

        [TestMethod]
        public void TestCreateJob()
        {
            var result = ApiClient.CreateJob("test_name", Language.English);
            Assert.AreEqual(32, result.JobId.ToString("N").Length);
            Assert.AreEqual(32, result.TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAuthorizeJob()
        {
            ApiClient.AuthorizeJob(JobId);
        }

        [TestMethod]
        public void TestDeleteJob()
        {
            TaskId = ApiClient.DeleteJob(JobId);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestGetJobInfo()
        {
            ApiClient.GetJobInfo(JobId);
        }

        [TestMethod]
        public void TestGetJobList()
        {
            ApiClient.GetJobList();
        }

        [TestMethod]
        public void TestGetElementList()
        {
            ApiClient.GetElementList(JobId);
        }

        [TestMethod]
        public void TestGetListOfElementLists()
        {
            ApiClient.GetListOfElementLists(JobId);
        }

        [TestMethod]
        public void TestGetMedia()
        {
            // Add media to job first
            ApiClient.AddMediaToJob(JobId, Config.SampleVideoUri);
            // Test get media
            ApiClient.GetMedia(JobId);
        }

        [TestMethod]
        public void TestGetTranscript()
        {
            ApiClient.GetTranscript(JobId);
        }

        [TestMethod]
        public void TestGetCaption()
        {
            ApiClient.GetCaption(JobId, CaptionFormat.Srt);
        }

        [TestMethod]
        public void TestGetCaptionBuildUrl()
        {
            var options = new CaptionOptions(buildUri:true);
            var response = ApiClient.GetCaption(JobId, CaptionFormat.Srt, options);
            new Uri(response);
        }

        [TestMethod]
        public void TestPerformTranscription()
        {
            ApiClient.AddMediaToJob(JobId, Config.SampleVideoUri);
            var callbackUri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            TaskId = ApiClient.PerformTranscription(JobId, Fidelity.Premium, JobPriority.Standard, callbackUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestPerformTranscriptionCallbackUrlEncoding()
        {
            var callbackUri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            const string encodedUri = "callback_url=http%3a%2f%2ffake-callback.com%2faction%3fapi_token%3d1234%26job_id%3d%7bjob_id%7d";
            ApiClient.AddMediaToJob(JobId, Config.SampleVideoUri);
            TaskId = ApiClient.PerformTranscription(JobId, Fidelity.Premium, JobPriority.Standard, callbackUri);
            // Last log entry will contain the callback to perform_transcription
            Assert.IsTrue(MemoryTarget.Logs.Last().Contains(encodedUri));
        }

        [TestMethod]
        public void TestAddMediaToJobUrl()
        {
            TaskId = ApiClient.AddMediaToJob(JobId, Config.SampleVideoUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAddMediaToJobEmbedded()
        {
            TaskId = ApiClient.AddEmbeddedMediaToJob(JobId, Config.SampleVideoUri);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAddMediaToJobFile()
        {
            var fs = new FileStream(Config.SampleVideoFilePath, FileMode.Open);
            TaskId = ApiClient.AddMediaToJob(JobId, fs);
            Assert.AreEqual(32, TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void TestAggregateStatistics()
        {
            var result = ApiClient.AggregateStatistics(Metric.BillableMinutesTotal | Metric.BillableMinutesProfessional,
                "month", new DateTime(2015, 6, 25), new DateTime(2015, 7, 25), "*");

            Assert.IsNotNull(result);
        }
    }
}
