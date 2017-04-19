using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Response
{
    public class RainBirdQueryResponse:RainBirdResponse
    {
        [JsonProperty("question")]
        public Question Question { get; set; }
    }

    public class Question
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("dataType")]
        public string DataType { get; set; }
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
        [JsonProperty("prompt")]
        public string Prompt { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("plural")]
        public bool Plural { get; set; }
        [JsonProperty("allowCF")]
        public bool AllowCf { get; set; }
        [JsonProperty("allowUnknown")]
        public bool AllowUnknown { get; set; }
        [JsonProperty("knownAnswers")]
        public List<string> KnownAnswers { get; set; } 
        [JsonProperty("concepts")]
        public List<Concept> Concepts { get; set; } 
    }

    public class Concept
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("conceptType")]
        public string ConceptType { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
        [JsonProperty("dynamic")]
        public string Dynamic { get; set; }
        [JsonProperty("restrictions")]
        public List<object> Restrictions { get; set; }
        [JsonProperty("metadata")]
        public object Metadata { get; set; }
    }
}
