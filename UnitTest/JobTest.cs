using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cielo24;
using Cielo24.JSON.ElementList;
using Cielo24.JSON.Job;
using Cielo24.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class JobTest : ActionsTest
    {
        protected Guid jobId = Guid.Empty;
        protected Guid taskId = Guid.Empty;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            // Always start with a fresh job
            jobId = actions.CreateJob(apiToken).JobId;
        }

        [TestMethod]
        public void testOptions()
        {
            CaptionOptions options = new CaptionOptions();
            options.CaptionBySentence = true;
            options.ForceCase = Case.UPPER;
            String[] array = { "build_url=true", "dfxp_header=header" };
            options.PopulateFromArray(array);
            Assert.AreEqual("build_url=true&caption_by_sentence=true&dfxp_header=header&force_case=upper", options.ToQuery().ToLower());
        }

        [TestMethod]
        public void testCreateJob()
        {
            CreateJobResult result = actions.CreateJob(apiToken, "test_name", Language.ENGLISH);
            Assert.AreEqual(32, result.JobId.ToString("N").Length);
            Assert.AreEqual(32, result.TaskId.ToString("N").Length);
        }

        [TestMethod]
        public void testAuthorizeJob()
        {
            actions.AuthorizeJob(apiToken, jobId);
        }

        [TestMethod]
        public void testDeleteJob()
        {
            taskId = actions.DeleteJob(apiToken, jobId);
            Assert.AreEqual(32, taskId.ToString("N").Length);
        }

        [TestMethod]
        public void testGetJobInfo()
        {
            Job info = actions.GetJobInfo(apiToken, jobId);
        }

        [TestMethod]
        public void testGetJobList()
        {
            JobList list = actions.GetJobList(apiToken);
        }

        [TestMethod]
        public void testGetElementList()
        {
            ElementList list = actions.GetElementList(apiToken, jobId);
        }

        [TestMethod]
        public void testGetListOfElementLists()
        {
            List<ElementListVersion> list = actions.GetListOfElementLists(apiToken, jobId);
        }

        [TestMethod]
        public void testGetMedia()
        {
            // Add media to job first
            actions.AddMediaToJob(apiToken, jobId, config.sampleVideoUri);
            // Test get media
            Uri uri = actions.GetMedia(apiToken, jobId);
        }

        [TestMethod]
        public void testGetTranscript()
        {
            actions.GetTranscript(apiToken, jobId);
        }

        [TestMethod]
        public void testGetCaption()
        {
            actions.GetCaption(apiToken, jobId, CaptionFormat.SRT);
        }

        [TestMethod]
        public void testGetCaptionBuildUrl()
        {
            CaptionOptions options = new CaptionOptions(buildUri:true);
            string response = actions.GetCaption(apiToken, jobId, CaptionFormat.SRT, options);
            Uri uri = new Uri(response);
        }

        [TestMethod]
        public void testPerformTranscription()
        {
            actions.AddMediaToJob(apiToken, jobId, config.sampleVideoUri);
            Uri callback_uri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            taskId = actions.PerformTranscription(apiToken, jobId, Fidelity.PREMIUM, Priority.STANDARD, callback_uri);
            Assert.AreEqual(32, taskId.ToString("N").Length);
        }

        [TestMethod]
        public void testPerformTranscriptionCallbackUrlEncoding()
        {
            Uri callbackUri = new Uri("http://fake-callback.com/action?api_token=1234&job_id={job_id}");
            string encodedUri = "callback_url=http:%2F%2Ffake-callback.com%2Faction%3Fapi_token%3D1234%26job_id%3D{job_id}";
            actions.AddMediaToJob(apiToken, jobId, config.sampleVideoUri);
            taskId = actions.PerformTranscription(apiToken, jobId, Fidelity.PREMIUM, Priority.STANDARD, callbackUri);
            // Last log entry will contain the callback to perform_transcription
            Assert.IsTrue(memoryTarget.Logs.Last().Contains(encodedUri));
        }

        [TestMethod]
        public void testAddMediaToJobUrl()
        {
            taskId = actions.AddMediaToJob(apiToken, jobId, config.sampleVideoUri);
            Assert.AreEqual(32, taskId.ToString("N").Length);
        }

        [TestMethod]
        public void testAddMediaToJobEmbedded()
        {
            taskId = actions.AddEmbeddedMediaToJob(apiToken, jobId, config.sampleVideoUri);
            Assert.AreEqual(32, taskId.ToString("N").Length);
        }

        [TestMethod]
        public void testAddMediaToJobFile()
        {
            FileStream fs = new FileStream(config.sampleVideoFilePath, FileMode.Open);
            taskId = actions.AddMediaToJob(apiToken, jobId, fs);
            Assert.AreEqual(32, taskId.ToString("N").Length);
        }
    }
}
