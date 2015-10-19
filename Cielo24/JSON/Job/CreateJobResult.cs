﻿using System;
using Newtonsoft.Json;

namespace Cielo24.JSON.Job
{
    public class CreateJobResult : JsonBase
    {
        [JsonProperty("JobId")]
        public Guid JobId { get; set; }
        [JsonProperty("TaskId")]
        public Guid TaskId { get; set; }
    }
}
