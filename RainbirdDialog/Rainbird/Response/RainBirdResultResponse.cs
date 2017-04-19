using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Response
{
    public class RainBirdResultResponse:RainBirdResponse
    {
        [JsonProperty("result")]
        public List<RainBirdResult> Results { get; set; }
    }

    public class RainBirdResult
    {
        [JsonProperty("relationshipType")]
        public string RelationshipType { get; set; }
        [JsonProperty("certainty")]
        public int Certainty { get; set; }
        [JsonProperty("conditions")]
        public List<string> Conditions { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("object")]
        public object Object { get; set; }
        [JsonProperty("sessionID")]
        public string SessionId { get; set; }
        [JsonProperty("objectMetadata")]
        public object ObjectMetadata { get; set; }
    }
}
