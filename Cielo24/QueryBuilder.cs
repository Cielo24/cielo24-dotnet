using System;
using System.Collections.Generic;
using System.Web;

namespace Cielo24
{
    class QueryBuilder
    {
        private readonly Dictionary<string, string> query;

        public string ServerUrl { get; }

        private QueryBuilder(string serverUrl, Dictionary<string, string> query)
        {
            ServerUrl = serverUrl;
            this.query = query;
        }

        public QueryBuilder(string serverUrl)
            : this(serverUrl, new Dictionary<string, string>())
        {}

        public QueryBuilder AddJobId(Guid jobId)
        {
            Assert.NotEmpty(jobId, nameof(jobId));
            return Add("job_id", jobId.ToString("N"));
        }

        public QueryBuilder AddApiToken(Guid apiToken)
        {
            Assert.NotEmpty(apiToken, nameof(apiToken));
            return Add("api_token", apiToken.ToString("N"));
        }

        public QueryBuilder AddApiVersion(int version)
        {
            return Add("v", version.ToString());
        }

        public QueryBuilder Add(string key, string value)
        {
            Assert.StringRequired(key, nameof(key));
            Assert.StringRequired(value, nameof(value));

            var newQuery = new Dictionary<string, string>(query) {{key, value}};
            return new QueryBuilder(ServerUrl, newQuery);
        }

        public QueryBuilder AddOptional(string key, string value)
        {
            return string.IsNullOrEmpty(value) ? this : Add(key, value);
        }

        public QueryBuilder AddFromDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return this;

            var result = this;
            foreach (var pair in dictionary)
                result = result.Add(pair.Key, pair.Value);

            return result;
        }

        public string BuildUrl(string path)
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

        public string ToQuery()
        {
            return Utils.ToQuery(this.query);
        }
    }
}