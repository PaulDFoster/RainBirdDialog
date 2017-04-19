using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Response
{
    public class RainBirdResponse
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }
    }
}
