using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Wrapper.Models.RainBird.Response;
using Wrapper.Models.RainBird.Request;
using Wrapper.Services;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RainbirdDialog 
{

    [Serializable]
    public class RainbirdDialog : IDialog<object>
    {
        public enum RainBirdPostType { Query, Response, Inject};

        private string KMID;
        private string OpeningQuery;
        private const string BaseUrl = @"https://enterprise-api.rainbird.ai";
        private const string MapId = @"fe08d53b-189d-4bac-8a15-8aced896b1b5";
        private const string ApiKey = @"2f57db7c-a2d3-46d3-9526-367670081890";
        private string ConsumerKey = @""; // Not sure where this comes from or when it is required?
        private static string _sessionId;

        public RainbirdDialog(string _kmid, string openingQuery)
        {
            // TODO Validate neither are null
            KMID = _kmid;
            OpeningQuery = openingQuery;

        }

        private string Start(string goalId)
        {
            var finalUrl = $"{BaseUrl}/start/{goalId}";
            var client = new HttpClientService<RainBirdStartResponse>();
            var startResponse = client.Get(finalUrl, ApiKey, "");

            return startResponse.Id;
        }

        public async Task StartAsync(IDialogContext context)
        {
            // /Start rainbird dialog
            // require the KMID
            if (KMID == null) throw new Exception("No Knowledge Map ID set for this conversation");

            // Run /Start with Rainbird and store the Session ID returned
            context.UserData.SetValue<string>("RainbirdSessionId", Start(KMID));

            await context.PostAsync(RainBirdPost(context, OpeningQuery, RainBirdPostType.Query));

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            // This should now be a response to a RainBird question

            // Need to pass it back to RainBird to get the next step

            // Execute the appropriate Rainbird action

            // Assuming this is a query response (Not Inject and not able to start a new Query (this done by LUIS))
            // Not testing for mandatory first question

            var message = await argument;

            // Constructing users response for RainBird

            RainBirdResultResponse rrr = new RainBirdResultResponse();
            RainBirdResult r = new RainBirdResult();
            string outStr = "";

            if (context.UserData.TryGetValue("RainbirdSessionId", out outStr))
            {
                r.SessionId = outStr;
            }
            else
            {
                r.SessionId = "";
            }

            if (context.UserData.TryGetValue("RainbirdLastSubject", out outStr))
            {
                r.Subject = outStr;
            }
            else
            {
                r.Subject = "";
            }

            if (!context.UserData.TryGetValue("RainbirdLastRelationship", out outStr))
            {
                r.RelationshipType = outStr;
            }
            else
            {
                r.RelationshipType = "";
            }

            if (!context.UserData.TryGetValue("RainbirdLastObject", out outStr))
            {
                r.ObjectMetadata = outStr;
            }
            else
            {
                r.ObjectMetadata = "";
            }

            r.Certainty = 100;
            r.Object = message.Text;

            rrr.Results.Add(r);

            // Posting Repsonse to RainBird
            // Posting RainBird Response to User
            string jsonobj = JsonConvert.SerializeObject(rrr);
            await context.PostAsync(RainBirdPost(context, jsonobj, RainBirdPostType.Response));

            context.Wait(MessageReceivedAsync);
        }

        public IMessageActivity RainBirdPost(IDialogContext context, string jsonObj, RainBirdPostType postType)
        {

            // retrieve context.UserData.SetValue<string>("RainbirdSession", Start(KMID));

            string finalUrl = "";

            switch (postType)
            {
                case RainBirdPostType.Query:
                    finalUrl = $"{BaseUrl}/{_sessionId}/query";
                    break;
                case RainBirdPostType.Response:
                    finalUrl = $"{BaseUrl}/{_sessionId}/response";
                    break;
                case RainBirdPostType.Inject:
                    finalUrl = "";
                    break;
            }
            

            var client = new HttpClientService<string>();
            var result = client.PostInsights(jsonObj, finalUrl, ApiKey, "", true);

            // Construct the Message for the next dialog step
            // Can be a question from RainBird, a result from RainBird or an exception from RainBird
            var message = context.MakeMessage();

            if (result.Contains("question"))
            {
                RainBirdQueryResponse  r = JsonConvert.DeserializeObject<RainBirdQueryResponse>(result);

                context.UserData.SetValue<string>("RainbirdLastSubject", r.Question.Subject);
                context.UserData.SetValue<string>("RainbirdLastRelationship", r.Question.Relationship);
                if(r.Question.Type.Contains("first"))
                    context.UserData.SetValue<bool>("RainbirdLastAnswerRequired", true);

                if (r.Question.Concepts.Count>0)
                {
                    context.UserData.SetValue<string>("RainbirdLastObject", r.Question.Concepts[0].ConceptType);

                    HeroCard hc = new HeroCard();
                    hc.Text = r.Question.Prompt;
                    foreach (Concept x in r.Question.Concepts)
                    {
                        CardAction ca = new CardAction("imBack", x.Name, null, x.Value);
                        hc.Buttons.Add(ca);
                    }
                    message.Attachments.Add(hc.ToAttachment());

                }
                else if (r.Question.DataType == "number")
                {
                    //PromptDialog.Number(context, ResumeAndPromptPlatformAsync, r.Question.Prompt);
                    message.Text = r.Question.Prompt;

                }
                else if (r.Question.DataType == "date")
                {
                    // No PromptDialog eqv for PromptDialog.Time
                    // builder.Prompts.time(session, rbQuestion.prompt);
                    message.Text = r.Question.Prompt;

                }
            }

            if (result.Contains("result"))
            {
                RainBirdResultResponse r = JsonConvert.DeserializeObject<RainBirdResultResponse>(result);
                message.Text = "Results found";
            }
            
            if (result.Contains("exception"))
            {
                RainBirdExceptionResponse r = JsonConvert.DeserializeObject<RainBirdExceptionResponse>(result);
                message.Text = r.Exception;
            }



            return message;

        }

    }
}