using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Request
{
    public class RainBirdResponseRequest
    {
        [JsonProperty("answers")]
        public List<RainBirdRequest> Answers { get; set; } 
    }
}
