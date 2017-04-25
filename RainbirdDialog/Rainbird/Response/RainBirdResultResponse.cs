using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wrapper.Models.RainBird.Response
{
    public class RainBirdResultResponse:RainBirdResponse
    {
        [JsonProperty("result")]
        public List<RainBirdResult> Results { get; set; }
    }
    //{"result":[{"objectMetadata":{"en":[{"data":"The Gold Travel Account provides:\n\n* Overdraft facility of £3,000\n* Phone Insurance\n* Car Breakdown Cover\n* Travel Insurance\n* Preferential Foreign Exchange Rates","dataType":"md"}]},"object":"Gold Travel Account","subject":"the customer","factID":"WA:RF:4fecf7719b6ccfba2a6864f13772726c","relationship":"recommended","relationshipType":"recommended","certainty":72}],"stats":{"getDBFact":{"calls":1228,"items":1198,"ms":3590},"callDatasource":{"calls":0,"ms":0},"ensureCache":{"ms":16},"setDBFact":{"calls":44,"ms":573},"totalMS":13228,"approxEngineMS":135,"invocationStartTime":1493136878953},"sid":"91dcdd31-dadb-4e6b-b12b-1b41ac59da93"}

    public class RainBirdResult
    {
        [JsonProperty("factID")]
        public string FactId { get; set; }
        [JsonProperty("relationship")]
        public string Relationship { get; set; }
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
        public RainBirdObjectMetadata ObjectMetadata { get; set; }
    }

    public class RainBirdObjectMetadata
    {
        [JsonProperty("en")]
        public List<RainBirdEndata> en { get; set; }

    }

    public class RainBirdEndata
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("datatype")]
        public string DataType { get; set; }
    }
}
