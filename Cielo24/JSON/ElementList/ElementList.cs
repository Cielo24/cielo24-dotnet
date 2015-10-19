using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cielo24.JSON.ElementList
{
    public class ElementList : JsonBase
    {
        [JsonProperty("version")]
        public int? Version { get; set; }
        [JsonProperty("start_time")]
        public int? StartTime { get; set; }      // Milliseconds
        [JsonProperty("end_time")]
        public int? EndTime { get; set; }        // Milliseconds
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("segments")]
        public List<Segment> Segments { get; set; }
        [JsonProperty("speakers")]
        public List<Speaker> Speakers { get; set; }
        [JsonProperty("keywords")]
        public Dictionary<string, MetaToken> Keywords { get; set; }
        [JsonProperty("topics")]
        public Dictionary<string, MetaToken> Topics { get; set; }
        [JsonProperty("entities")]
        public Dictionary<string, MetaToken> Entities { get; set; }
    }
}