using System;
using System.Collections.Generic;
using System.IO;
using Cielo24.JSON.ElementList;
using Cielo24.JSON.Job;
using Cielo24.Options;
using Newtonsoft.Json;

namespace Cielo24
{
    public class ApiClient
    {
        public const int ApiVersion = 1;
        public const string DefaultServerUrl = "https://api.cielo24.com";

        private readonly WebUtils web = new WebUtils();

        private QueryBuilder QueryBuilder { get; }

        public Guid ApiToken { get; }
        public string ServerUrl => QueryBuilder.ServerUrl;        

        private ApiClient(string serverUrl, Guid apiToken)
        {
            ApiToken = apiToken;
            QueryBuilder = new QueryBuilder(serverUrl)
                .AddApiVersion(ApiVersion)
                .AddApiToken(apiToken);
        }

        /// <summary>
        /// Performs a Login action.
        /// </summary>
        /// <param name="username">username string</param>
        /// <param name="password">password string</param>
        /// <param name="serverUrl"></param>
        /// <returns>new instance of API client</returns>
        public static ApiClient Login(string username, string password, string serverUrl = DefaultServerUrl)
        {
            Assert.StringRequired(username, nameof(username));
            Assert.StringRequired(password, nameof(password));

            var url = new QueryBuilder(serverUrl)
                .AddApiVersion(ApiVersion)
                .Add("username", username)
                .Add("password", password)
                .BuildUrl(Paths.Login);
           
            var serverResponse = new WebUtils().HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            var apiToken = new Guid(response["ApiToken"]);

            return new ApiClient(serverUrl, apiToken);
        }

        /// <summary>
        /// Performs a Login action.
        /// </summary>
        /// <param name="username">username string</param>
        /// <param name="secureKey">secure key</param>
        /// <returns>new instance of API client</returns>
        public static ApiClient Login(string username, Guid secureKey, string serverUrl = DefaultServerUrl)
        {
            Assert.StringRequired(username, nameof(username));
            Assert.NotEmpty(secureKey, nameof(secureKey));

            var url = new QueryBuilder(serverUrl)
                .AddApiVersion(ApiVersion)
                .Add("username", username)
                .Add("secureKey", secureKey.ToString("N"))
                .BuildUrl(Paths.Login);
            
            var serverResponse = new WebUtils().HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            var apiToken = new Guid(response["ApiToken"]);

            return new ApiClient(serverUrl, apiToken);
        }
        
        /// <summary>
        /// Logs out the current user.
        /// </summary>
        public void Logout()
        {
            var url = QueryBuilder.BuildUrl(Paths.Logout);
            web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
        }

        /// <summary>
        /// Updates password for the user.
        /// </summary>
        /// <param name="newPassword">new password for the user</param>
        /// <param name="subAccount">if specified, the password change will affect the provided subaccount</param>
        public void UpdatePassword(string newPassword, string subAccount = null)
        {
            Assert.StringRequired(newPassword, nameof(newPassword));
            
            var url = QueryBuilder.BuildUrl(Paths.UpdatePassword);
            var query = QueryBuilder
                .Add("new_password", newPassword)
                .AddOptional("username", subAccount)
                .ToQuery();

            web.HttpRequest(url, HttpMethod.Post, WebUtils.BasicTimeout, query);
        }
        
        /// <summary>
        /// Generates an API secure key.
        /// </summary>
        /// <param name="username">username string</param>
        /// <param name="forceNew">if true, it will generate a new API secure key, invalidating the previous one</param>
        /// <returns>generated API secure key</returns>
        public Guid GenerateApiKey(string username, bool forceNew = false)
        {
            Assert.StringRequired(username, nameof(username));

            var url = QueryBuilder
                .Add("account_id", username)
                .Add("force_new", forceNew.ToString())
                .BuildUrl(Paths.GenerateApiKey);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["ApiKey"]);
        }
        
        /// <summary>
        /// Deactivates the supplied secure API key.
        /// </summary>
        /// <param name="apiSecurekey">secure API key to be invalidated</param>
        public void RemoveApiKey(Guid apiSecurekey)
        {
            var url = QueryBuilder
                .Add("api_securekey", apiSecurekey.ToString("N"))
                .BuildUrl(Paths.RemoveApiKey);
            
            web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
        }

        /* Creates a new job. Returns an array of Guids where 'JobId' is the 0th element and 'TaskId' is the 1st element */
        /// <summary>
        /// Creates a new job. Returns an array of Guids where 'JobId' is the 0th element and 'TaskId' is the 1st element.
        /// </summary>
        /// <param name="jobName">name for the job, if not provided it will be autogenerated</param>
        /// <param name="language">language for the job media, English by default</param>
        /// <param name="externalId">external ID for the job</param>
        /// <param name="subAccount">if provided, job will be created for a sub-account</param>
        /// <returns>result of creating a job</returns>
        public CreateJobResult CreateJob(string jobName = null, Language? language = Language.English, string externalId = null, string subAccount = null)
        {
            var url = QueryBuilder
                .AddOptional("job_name", jobName)
                .AddOptional("language", language?.GetDescription())
                .AddOptional("external_id", externalId)
                .AddOptional("username", subAccount)
                .BuildUrl(Paths.CreateJob);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<CreateJobResult>(serverResponse);
        }
        
        /// <summary>
        /// Authorizes a job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        public void AuthorizeJob(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            var url = QueryBuilder
                .AddJobId(jobId)
                .BuildUrl(Paths.AuthorizeJob);
            
            web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
        }
        
        /// <summary>
        /// Deletes a job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Guid DeleteJob(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));
            
            var response = GetJobResponse<Dictionary<string, string>>(jobId, Paths.DeleteJob);
            return new Guid(response["TaskId"]);
        }
        
        /// <summary>
        /// Gets information about a job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Job GetJobInfo(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            return GetJobResponse<Job>(jobId, Paths.GetJobInfo);
        }
        
        /// <summary>
        /// Gets a list of jobs.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public JobList GetJobList(JobListOptions options = null)
        {
            var url = QueryBuilder
                .AddFromDictionary(options?.GetDictionary())
                .BuildUrl(Paths.GetJobList);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var jobList = Utils.Deserialize<JobList>(serverResponse);

            return jobList;
        }

        /// <summary>
        /// Uploads a file from fileStream to job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public Guid AddMediaToJob(Guid jobId, Stream fileStream)
        {
            Assert.NotNull(fileStream, nameof(fileStream));

            var url = QueryBuilder
                .AddJobId(jobId)
                .BuildUrl(Paths.AddMediaToJob);
            
            var serverResponse = web.UploadData(url, fileStream, "video/mp4");
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        /// <summary>
        /// Provides job with jobId a url to media.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public Guid AddMediaToJob(Guid jobId, Uri mediaUrl)
        {
            Assert.NotEmpty(jobId, nameof(jobId));
            Assert.NotEmpty(mediaUrl, nameof(mediaUrl));

            return SendMediaUrl(jobId, mediaUrl, Paths.AddMediaToJob);
        }

        /// <summary>
        /// Provides job with jobId a url to media.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public Guid AddEmbeddedMediaToJob(Guid jobId, Uri mediaUrl)
        {
            Assert.NotEmpty(jobId, nameof(jobId));
            Assert.NotEmpty(mediaUrl, nameof(mediaUrl));

            return SendMediaUrl(jobId, mediaUrl, Paths.AddEmbeddedMediaToJob);
        }

        /// <summary>
        /// Returns a Uri to the media from job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Uri GetMedia(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            var response = GetJobResponse<Dictionary<string, string>>(jobId, Paths.GetMedia);
            return new Uri(response["MediaUrl"]);
        }

        /// <summary>
        /// Makes a PerformTranscription call.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fidelity"></param>
        /// <param name="priority"></param>
        /// <param name="callbackUri"></param>
        /// <param name="turnaroundHours"></param>
        /// <param name="targetLanguage"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Guid PerformTranscription(Guid jobId, Fidelity fidelity, JobPriority? priority = null,
            Uri callbackUri = null, int? turnaroundHours = null,
            Language? targetLanguage = null, PerformTranscriptionOptions options = null)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            var url = QueryBuilder
                .AddJobId(jobId)
                .Add("transcription_fidelity", fidelity.GetDescription())
                .AddOptional("priority", priority?.GetDescription())
                .AddOptional("callback_url", callbackUri?.ToString())
                .AddOptional("turnaround_hours", turnaroundHours?.ToString())
                .AddOptional("target_language", targetLanguage?.GetDescription())
                .AddOptional("options", options != null ? JsonConvert.SerializeObject(options.GetDictionary()) : null)
                .BuildUrl(Paths.PerformTranscription);
                            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        /// <summary>
        /// Returns a transcript from a job with jobId.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="transcriptOptions"></param>
        /// <returns></returns>
        public string GetTranscript(Guid jobId, TranscriptOptions transcriptOptions = null)
        { 
            Assert.NotEmpty(jobId, nameof(jobId));

            var url = QueryBuilder
                .AddJobId(jobId)
                .AddFromDictionary(transcriptOptions?.GetDictionary())
                .BuildUrl(Paths.GetTranscript);
            
            return web.HttpRequest(url, HttpMethod.Get, WebUtils.DownloadTimeout);
        }

        /// <summary>
        /// Returns a caption from a job with jobId OR if buildUri is true, returns a string representation of the uri.
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="captionFormat"></param>
        /// <param name="captionOptions"></param>
        /// <returns></returns>
        public string GetCaption(Guid jobId, CaptionFormat captionFormat, CaptionOptions captionOptions = null)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            var url = QueryBuilder
                .AddJobId(jobId)
                .Add("caption_format", captionFormat.GetDescription())
                .AddFromDictionary(captionOptions?.GetDictionary())
                .BuildUrl(Paths.GetCaption);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.DownloadTimeout);
            if (captionOptions?.BuildUrl == null || captionOptions.BuildUrl != true)
                return serverResponse; // Caption text
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);
            return response["CaptionUrl"];
        }

        /// <summary>
        /// Returns an element list.
        /// </summary>
        /// <param name="apiToken"></param>
        /// <param name="jobId"></param>
        /// <param name="elementListVersion"></param>
        /// <returns></returns>
        public ElementList GetElementList(Guid jobId, DateTime? elementListVersion = null)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            var url = QueryBuilder
                .AddJobId(jobId)
                .AddOptional("elementlist_version", Utils.DateToIsoFormat(elementListVersion))
                .BuildUrl(Paths.GetElementList);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.DownloadTimeout);
            return Utils.Deserialize<ElementList>(serverResponse);
        }

        /// <summary>
        /// Returns a list of elements lists.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public List<ElementListVersion> GetListOfElementLists(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));

            return GetJobResponse<List<ElementListVersion>>(jobId, Paths.GetListOfElementLists);
        }

        /// <summary>
        /// Return aggregate statistics.
        /// </summary>
        /// <param name="metrics"></param>
        /// <param name="groupBy"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="subAccount"></param>
        /// <returns></returns>
        public Dictionary<string, object> AggregateStatistics(List<string> metrics,
            string groupBy, DateTime? startDate, DateTime? endDate, string subAccount)
        {
            var url = QueryBuilder
                .AddOptional("metrics", Utils.JoinQuoteList(metrics, ","))
                .AddOptional("group_by", groupBy)
                .AddOptional("start_date", Utils.DateToIsoFormat(startDate))
                .AddOptional("end_date", Utils.DateToIsoFormat(endDate))
                .AddOptional("account_id", subAccount)
                .BuildUrl(Paths.AggregateStatistics);
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<Dictionary<string, object>>(serverResponse);
        }

        private Guid SendMediaUrl(Guid jobId, Uri mediaUrl, string path)
        {
            var url = QueryBuilder
                .AddJobId(jobId)
                .Add("media_url", mediaUrl.ToString())
                .BuildUrl(path);            
            
            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            var response = Utils.Deserialize<Dictionary<string, string>>(serverResponse);

            return new Guid(response["TaskId"]);
        }

        private T GetJobResponse<T>(Guid jobId, string path)
        {
            var url = QueryBuilder
                .AddJobId(jobId)
                .BuildUrl(path);

            var serverResponse = web.HttpRequest(url, HttpMethod.Get, WebUtils.BasicTimeout);
            return Utils.Deserialize<T>(serverResponse);
        }        
    }
}