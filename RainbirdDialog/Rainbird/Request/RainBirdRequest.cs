using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Request
{
    public class RainBirdRequest
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
        [JsonProperty("object")]
        public object Object { get; set; }
        [JsonProperty("answer")]
        public string Answer { get; set; }
        [JsonProperty("cf")]
        public int Certainty { get; set; }
    }
}
