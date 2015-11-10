using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Cielo24.JSON.ElementList;
using Cielo24.JSON.Job;
using Cielo24.Options;
using Newtonsoft.Json;

namespace Cielo24
{
    public class ApiClient
    {
        public const int Version = 1;
        public string ServerUrl { get; set; } = "https://api.cielo24.com";
        private readonly WebUtils web = new WebUtils();

        private const string LoginPath = "/api/account/login";
        private const string LogoutPath = "/api/account/logout";
        private const string UpdatePasswordPath = "/api/account/update_password";
        private const string GenerateApiKeyPath = "/api/account/generate_api_key";
        private const string RemoveApiKeyPath = "/api/account/remove_api_key";
        private const string CreateJobPath = "/api/job/new";
        private const string AuthorizeJobPath = "/api/job/authorize";
        private const string DeleteJobPath = "/api/job/del";
        private const string GetJobInfoPath = "/api/job/info";
        private const string GetJobListPath = "/api/job/list";
        private const string AddMediaToJobPath = "/api/job/add_media";
        private const string AddEmbeddedMediaToJobPath = "/api/job/add_media_url";
        private const string GetMediaPath = "/api/job/media";
        private const string PerformTranscriptionPath = "/api/job/perform_transcription";
        private const string GetTranscriptPath = "/api/job/get_transcript";
        private const string GetCaptionPath = "/api/job/get_caption";
        private const string GetElementListPath = "/api/job/get_elementlist";
        private const string GetListOfElementListsPath = "/api/job/list_elementlists";
        private const string AggregateStatisticsPath = "/api/job/aggregate_statistics";

        public ApiClient() { }

        public ApiClient(string uri)
        {
            ServerUrl = uri;
        }

        //////////////////////
        /// ACCESS CONTROL ///
        //////////////////////

        /* Performs a Login action. If useHeaders is true, puts username and password into HTTP headers */
        public Guid Login(string username, string password, bool useHeaders = false)
        {
            Assert.StringRequired(username, nameof(username));            
            Assert.StringRequired(password, nameof(password));

            var queryDictionary = InitVersionDict();
            var headers = new Dictionary<string, string>();

            if (useHeaders)
            {
                headers.Add("x-auth-user", username);
                headers.Add("x-auth-password", password);
            }
            else
            {
                queryDictionary.Add("username", username);
                queryDictionary.Add("password", password);
            }

            var requestUri = Utils.BuildUri(ServerUrl, LoginPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout, headers);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["ApiToken"]);
        }

        /* Performs a Login action. If useHeaders is true, puts securekey into HTTP headers */
        public Guid Login(string username, Guid securekey, bool useHeaders = false)
        {
            Assert.StringRequired(username, nameof(username));
            Assert.NotEmpty(securekey, nameof(securekey));

            var queryDictionary = InitVersionDict();
            var headers = new Dictionary<string, string>();

            if (useHeaders)
            {
                headers.Add("x-auth-user", username);
                headers.Add("x-auth-securekey", securekey.ToString("N"));
            }
            else
            {
                queryDictionary.Add("username", username);
                queryDictionary.Add("securekey", securekey.ToString("N"));
            }

            var requestUri = Utils.BuildUri(ServerUrl, LoginPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout, headers);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["ApiToken"]);
        }

        /* Performs a Logout action */
        public void Logout(Guid apiToken)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));

            var queryDictionary = InitAccessReqDict(apiToken);
            var requestUri = Utils.BuildUri(ServerUrl, LogoutPath, queryDictionary);
            web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout); // Nothing returned
        }

        /* Updates password */
        public void UpdatePassword(Guid apiToken, string newPassword, string subAccount = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.StringRequired(newPassword, nameof(newPassword));

            var queryDictionary = InitAccessReqDict(apiToken);
            queryDictionary.Add("new_password", newPassword);
            if (subAccount != null) { queryDictionary.Add("username", subAccount); } // username parameter named sub_account for clarity

            var requestUri = Utils.BuildUri(ServerUrl, UpdatePasswordPath, queryDictionary);
            web.HttpRequest(requestUri, HttpMethod.Post, WebUtils.BasicTimeout, Utils.ToQuery(queryDictionary)); // Nothing returned
        }

        /* Returns a new Secure API key */
        public Guid GenerateApiKey(Guid apiToken, string username, bool forceNew = false)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.StringRequired(username, nameof(username));

            var queryDictionary = InitAccessReqDict(apiToken);
            queryDictionary.Add("account_id", username);
            queryDictionary.Add("force_new", forceNew.ToString());

            var requestUri = Utils.BuildUri(ServerUrl, GenerateApiKeyPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["ApiKey"]);
        }

        /* Deactivates the supplied Secure API key */
        public void RemoveApiKey(Guid apiToken, Guid apiSecurekey)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));

            var queryDictionary = InitAccessReqDict(apiToken);
            queryDictionary.Add("api_securekey", apiSecurekey.ToString("N"));

            var requestUri = Utils.BuildUri(ServerUrl, RemoveApiKeyPath, queryDictionary);
            web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout); // Nothing returned
        }

        ///////////////////
        /// JOB CONTROL ///
        ///////////////////

        /* Creates a new job. Returns an array of Guids where 'JobId' is the 0th element and 'TaskId' is the 1st element */
        public CreateJobResult CreateJob(Guid apiToken, string jobName = null, Language? language = Language.English, string externalId = null, string subAccount = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            
            var queryDictionary = InitAccessReqDict(apiToken);
            if (jobName != null) { queryDictionary.Add("job_name", jobName); }
            if (language != null) { queryDictionary.Add("language", language.GetDescription()); }
            if (externalId != null) { queryDictionary.Add("external_id", externalId); }
            if (subAccount != null) { queryDictionary.Add("username", subAccount); } // username parameter named sub_account for clarity

            var requestUri = Utils.BuildUri(ServerUrl, CreateJobPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<CreateJobResult>(serverResponse);
        }

        /* Authorizes a job with jobId */
        public void AuthorizeJob(Guid apiToken, Guid jobId)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            var requestUri = Utils.BuildUri(ServerUrl, AuthorizeJobPath, queryDictionary);
            web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout); // Nothing returned
        }

        /* Deletes a job with jobId */
        public Guid DeleteJob(Guid apiToken, Guid jobId)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var response = GetJobResponse<Dictionary<string, string>>(apiToken, jobId, DeleteJobPath);
            return new Guid(response["TaskId"]);
        }

        /* Gets information about a job with jobId */
        public Job GetJobInfo(Guid apiToken, Guid jobId)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            return GetJobResponse<Job>(apiToken, jobId, GetJobInfoPath);
        }

        /* Gets a list of jobs */
        public JobList GetJobList(Guid apiToken, JobListOptions options = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
         
            var queryDictionary = InitAccessReqDict(apiToken);
            if (options != null) { queryDictionary = Utils.DictConcat(queryDictionary, options.GetDictionary()); }

            var requestUri = Utils.BuildUri(ServerUrl, GetJobListPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            var jobList = Utils.Deserialize<JobList>(serverResponse);

            return jobList;
        }

        /* Uploads a file from fileStream to job with jobId */
        public Guid AddMediaToJob(Guid apiToken, Guid jobId, Stream fileStream)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));
            Assert.NotNull(fileStream, nameof(fileStream));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            var requestUri = Utils.BuildUri(ServerUrl, AddMediaToJobPath, queryDictionary);
            var serverResponse = web.UploadData(requestUri, fileStream, "video/mp4");
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        /* Provides job with jobId a url to media */
        public Guid AddMediaToJob(Guid apiToken, Guid jobId, Uri mediaUrl)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));
            Assert.NotEmpty(mediaUrl, nameof(mediaUrl));

            return SendMediaUrl(apiToken, jobId, mediaUrl, AddMediaToJobPath);
        }

        /* Provides job with jobId a url to media */
        public Guid AddEmbeddedMediaToJob(Guid apiToken, Guid jobId, Uri mediaUrl)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));
            Assert.NotEmpty(mediaUrl, nameof(mediaUrl));

            return SendMediaUrl(apiToken, jobId, mediaUrl, AddEmbeddedMediaToJobPath);
        }

        /* Returns a Uri to the media from job with jobId */
        public Uri GetMedia(Guid apiToken, Guid jobId)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var response = GetJobResponse<Dictionary<string, string>>(apiToken, jobId, GetMediaPath);
            return new Uri(response["MediaUrl"]);
        }

        /* Makes a PerformTranscription call */
        public Guid PerformTranscription(Guid apiToken,
                                         Guid jobId,
                                         Fidelity fidelity,
                                         Priority? priority = null,
                                         Uri callbackUri = null,
                                         int? turnaroundHours = null,
                                         Language? targetLanguage = null,
                                         PerformTranscriptionOptions options = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            queryDictionary.Add("transcription_fidelity", fidelity.GetDescription());
            if (priority != null) { queryDictionary.Add("priority", priority.GetDescription()); }
            if (callbackUri != null) { queryDictionary.Add("callback_url", callbackUri.ToString()); }
            if (turnaroundHours != null) { queryDictionary.Add("turnaround_hours", turnaroundHours.ToString()); }
            if (targetLanguage != null) { queryDictionary.Add("target_language", targetLanguage.GetDescription()); }
            if (options != null) { queryDictionary.Add("options", JsonConvert.SerializeObject(options.GetDictionary())); }

            var requestUri = Utils.BuildUri(ServerUrl, PerformTranscriptionPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        /* Returns a transcript from a job with jobId */
        public string GetTranscript(Guid apiToken, Guid jobId, TranscriptOptions transcriptOptions = null)
        { 
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            if (transcriptOptions != null) { queryDictionary = Utils.DictConcat(queryDictionary, transcriptOptions.GetDictionary()); }

            var requestUri = Utils.BuildUri(ServerUrl, GetTranscriptPath, queryDictionary);
            return web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.DownloadTimeout); // Transcript text
        }

        /* Returns a caption from a job with jobId OR if buildUri is true, returns a string representation of the uri */
        public string GetCaption(Guid apiToken, Guid jobId, CaptionFormat captionFormat, CaptionOptions captionOptions = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            queryDictionary.Add("caption_format", captionFormat.GetDescription());
            if (captionOptions != null) { queryDictionary = Utils.DictConcat(queryDictionary, captionOptions.GetDictionary()); }

            var requestUri = Utils.BuildUri(ServerUrl, GetCaptionPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.DownloadTimeout);
            if (captionOptions?.BuildUrl == null || captionOptions.BuildUrl != true)
                return serverResponse; // Caption text
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);
            return response["CaptionUrl"];
        }

        /* Returns an element list */
        public ElementList GetElementList(Guid apiToken, Guid jobId, DateTime? elementListVersion = null)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            var queryDictionary = InitJobReqDict(apiToken, jobId);
            if (elementListVersion != null) { queryDictionary.Add("elementlist_version", Utils.DateToIsoFormat(elementListVersion)); }

            var requestUri = Utils.BuildUri(ServerUrl, GetElementListPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.DownloadTimeout);
            return Utils.Deserialize<ElementList>(serverResponse);
        }

        /* Returns a list of elements lists */
        public List<ElementListVersion> GetListOfElementLists(Guid apiToken, Guid jobId)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            Assert.NotEmpty(jobId, nameof(jobId));

            return GetJobResponse<List<ElementListVersion>>(apiToken, jobId, GetListOfElementListsPath);
        }

        /* Return aggregate statistics */
        public Dictionary<string, object> AggregateStatistics(Guid apiToken, List<string> metrics,
            string groupBy, DateTime? startDate, DateTime? endDate, string subAccount)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));

            var queryDictionary = InitAccessReqDict(apiToken);

            if (metrics != null)
                queryDictionary.Add("metrics", Utils.JoinQuoteList(metrics, ","));

            if (!string.IsNullOrEmpty(groupBy))
                queryDictionary.Add("group_by", groupBy);

            if (startDate != null)
                queryDictionary.Add("start_date", Utils.DateToIsoFormat(startDate));

            if (endDate != null)
                queryDictionary.Add("end_date", Utils.DateToIsoFormat(endDate));

            if (!string.IsNullOrEmpty(subAccount))
                queryDictionary.Add("account_id", subAccount);

            var requestUri = Utils.BuildUriRawString(ServerUrl, AggregateStatisticsPath, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<Dictionary<string, object>>(serverResponse);
        }

        //////////////////////////////
        /// PRIVATE HELPER METHODS ///
        //////////////////////////////

        /* Helper method for AddMediaToJob and AddEmbeddedMediaToJob methods */
        private Guid SendMediaUrl(Guid apiToken, Guid jobId, Uri mediaUrl, string path)
        {
            var queryDictionary = InitJobReqDict(apiToken, jobId);
            queryDictionary.Add("media_url", mediaUrl.ToString());

            var requestUri = Utils.BuildUri(ServerUrl, path, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        private T GetJobResponse<T>(Guid apiToken, Guid jobId, string path)
        {
            var queryDictionary = InitJobReqDict(apiToken, jobId);
            var requestUri = Utils.BuildUri(ServerUrl, path, queryDictionary);
            var serverResponse = web.HttpRequest(requestUri, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<T>(serverResponse);
        }

        /* Returns a dictionary with version, api_token and job_id key-value pairs (parameters used in almost every job-control action). */
        private static Dictionary<string, string> InitJobReqDict(Guid apiToken, Guid jobId)
        {
            var queryDictionary = InitAccessReqDict(apiToken);
            queryDictionary.Add("job_id", jobId.ToString("N"));
            return queryDictionary;
        }

        /* Returns a dictionary with version and api_token key-value pairs (parameters used in almost every access-control action). */
        private static Dictionary<string, string> InitAccessReqDict(Guid apiToken)
        {
            var queryDictionary = InitVersionDict();
            queryDictionary.Add("api_token", apiToken.ToString("N"));
            return queryDictionary;
        }

        /* Returns a dictionary with version key-value pair (parameter used in every action). */
        private static Dictionary<string, string> InitVersionDict()
        {
            var queryDictionary = new Dictionary<string, string> {{"v", Version.ToString()}};
            return queryDictionary;
        }
    }
}