using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wrapper.Services;
using Wrapper.Models.RainBird.Response;
using Newtonsoft.Json;
using Microsoft.Bot.Connector;

namespace RainbirdDialog.Rainbird
{
    public class RainbirdHelper
    {
        private const string BaseUrl = @"https://enterprise-api.rainbird.ai";
        private const string MapId = @"ecb58e2e-bd9f-477d-ac26-53a9f3c8a2c3";// fe08d53b-189d-4bac-8a15-8aced896b1b5
        private const string ApiKey = @"84bb676b-7ba5-4bf8-8fd2-c44ad4a61158"; //  1e8292d7-d484-4bd6-af7f-81ee721bb8df
        private static string _sessionId;

        private string Start(string goalId)
        {
            var finalUrl = $"{BaseUrl}/start/{goalId}";
            var client = new HttpClientService<RainBirdStartResponse>();
            var startResponse = client.Get(finalUrl, ApiKey, "");

            return startResponse.Id;
        }

        public string StartRainBirdConversation()
        {
            return Start(MapId);
        }

        public IMessageActivity Query(string jsonObj)
        {
            var finalUrl = $"{BaseUrl}/{_sessionId}/query";

            var client = new HttpClientService<string>();
            var result = client.PostInsights(jsonObj, finalUrl, ApiKey, "", true);

            if (result.Contains("question"))
            {
                return JsonConvert.DeserializeObject<RainBirdQueryResponse>(result);
            }
            else 
            {
                return JsonConvert.DeserializeObject<RainBirdResultResponse>(result);
            }
        }


    }
}