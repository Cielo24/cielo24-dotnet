namespace Cielo24
{
    static internal class Paths
    {
        public const string Login = "/api/account/login";
        public const string Logout = "/api/account/logout";
        public const string UpdatePassword = "/api/account/update_password";
        public const string GenerateApiKey = "/api/account/generate_api_key";
        public const string RemoveApiKey = "/api/account/remove_api_key";
        public const string CreateJob = "/api/job/new";
        public const string AuthorizeJob = "/api/job/authorize";
        public const string DeleteJob = "/api/job/del";
        public const string GetJobInfo = "/api/job/info";
        public const string GetJobList = "/api/job/list";
        public const string AddMediaToJob = "/api/job/add_media";
        public const string AddEmbeddedMediaToJob = "/api/job/add_media_url";
        public const string GetMedia = "/api/job/media";
        public const string PerformTranscription = "/api/job/perform_transcription";
        public const string GetTranscript = "/api/job/get_transcript";
        public const string GetCaption = "/api/job/get_caption";
        public const string GetElementList = "/api/job/get_elementlist";
        public const string GetListOfElementLists = "/api/job/list_elementlists";
        public const string AggregateStatistics = "/api/job/aggregate_statistics";
    }
}