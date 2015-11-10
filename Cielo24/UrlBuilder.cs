using System;
using System.Collections.Generic;
using System.Web;

namespace Cielo24
{
    class UrlBuilder
    {
        private readonly Dictionary<string, string> query;

        public string ServerUrl { get; }

        private UrlBuilder(string serverUrl, Dictionary<string, string> query)
            : this(serverUrl)
        {
            this.query = query;
        }

        public UrlBuilder(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        public UrlBuilder AddJobId(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));
            return Add("job_id", jobId.ToString("N"));
        }

        public UrlBuilder AddApiToken(Guid apiToken)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            return Add("api_token", apiToken.ToString("N"));
        }

        public UrlBuilder AddApiVersion(int version)
        {
            return Add("v", version.ToString());
        }

        public UrlBuilder Add(string key, string value)
        {
            Assert.StringRequired(key, nameof(key));
            Assert.StringRequired(value, nameof(value));

            var newQuery = new Dictionary<string, string>(query) {{key, value}};
            return new UrlBuilder(ServerUrl, newQuery);
        }

        public UrlBuilder AddOptional(string key, string value)
        {
            return string.IsNullOrEmpty(value) ? this : Add(key, value);
        }

        public UrlBuilder AddFromDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return this;

            foreach (var pair in dictionary)
                Add(pair.Key, pair.Value);

            return this;
        }

        public string Build(string path)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            foreach (var pair in query)
                queryParams.Add(pair.Key, pair.Value);

            var builder = new UriBuilder(ServerUrl)
            {
                Query = queryParams.ToString(),
                Path = path
            };
            return builder.ToString();
        }
    }
}