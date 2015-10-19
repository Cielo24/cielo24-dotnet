using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cielo24.JSON.ElementList
{
    public class MetaToken
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("url")]
        public Uri Uri { get; set; }
        [JsonProperty("time_ranges")]
        public List<TimeRange> TimeRanges { get; set; }
    }
}
