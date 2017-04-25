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
using System.Diagnostics;
using System.Web.Configuration;



namespace RainbirdDialog 
{

    [Serializable]
    public class RainbirdDialog : IDialog<bool>
    {
        public enum RainBirdPostType { Query, Response, Inject};

        private string KMID;
        private string OpeningQuery;
        private const string BaseUrl = @"https://enterprise-api.rainbird.ai";
        private string ApiKey = WebConfigurationManager.AppSettings["RainBirdAPIKey"];
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

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            if (RainBirdProperty("RainbirdSessionId", ref context) == "")
            {
                // This is the opening RainBird engagement
                // Start RainBird session
                // Open query
                // Run /Start with Rainbird and store the Session ID returned
                context.UserData.SetValue<string>("RainbirdSessionId", Start(KMID));

                await context.PostAsync(RainBirdPost(context, OpeningQuery, RainBirdPostType.Query));
            }
            else
            {

                // Assuming this is a query response (Not Inject and not able to start a new Query (this done by LUIS))
                // Not testing for mandatory first question

                var message = await argument;

                // Constructing users response for RainBird

                RainBirdResponseRequest rrr = new RainBirdResponseRequest();
                RainBirdRequest r = new RainBirdRequest();

                // Question Type defines the attribute to be answered

                switch (RainBirdProperty("RainbirdQuestionType", ref context))
                {
                        case "First Form":
                            r.Subject = RainBirdProperty("RainbirdLastSubject", ref context);
                            r.Object = RainBirdProperty("RainbirdLastObject", ref context);
                            r.Answer = message.Text;
                            break;
                        case "Second Form Object":  
                            r.Subject = RainBirdProperty("RainbirdLastSubject", ref context);
                            r.Object = message.Text;
                            break;
                        case "Second Form Subject":
                            r.Subject = message.Text; 
                            r.Object = RainBirdProperty("RainbirdLastObject", ref context);
                            break;
                }


                r.Relationship = RainBirdProperty("RainbirdLastRelationship", ref context);
                r.Certainty = 100;

                List<RainBirdRequest> answersList = new List<RainBirdRequest>();
                answersList.Add(r);
                rrr.Answers = answersList;

                // Posting Repsonse to RainBird
                // Posting RainBird Response to User
                string jsonobj = JsonConvert.SerializeObject(rrr);

                Debug.WriteLine(jsonobj);

                await context.PostAsync(RainBirdPost(context, jsonobj, RainBirdPostType.Response));
            }

            context.Wait(MessageReceivedAsync);
        }

        public IMessageActivity RainBirdPost(IDialogContext context, string jsonObj, RainBirdPostType postType)
        {
            // Construct the Message for the next dialog step
            // Can be a question from RainBird, a result from RainBird or an exception from RainBird
            var message = context.MakeMessage();

            string outStr = "";

            if (context.UserData.TryGetValue("RainbirdSessionId", out outStr))
            {
                _sessionId = outStr;
            }
            else
            {
                // If no session id should fail here not continue
                message.Text = "Unable to contact Rainbird knowledge engine. Query closed, try again later";
                return message;
            }

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
                    // To do: determine a fact and inject into conversation
                    finalUrl = "";
                    break;
            }
            

            var client = new HttpClientService<string>();

            Debug.WriteLine(finalUrl);

            var result = client.PostInsights(jsonObj, finalUrl, ApiKey, "", true);

            if (result.Contains("question"))
            {
                RainBirdQueryResponse  r = JsonConvert.DeserializeObject<RainBirdQueryResponse>(result);

                // Add Question type = First or Second. 
                context.UserData.SetValue<string>("RainbirdQuestionType", r.Question.Type);
                context.UserData.SetValue<string>("RainbirdLastSubject", r.Question.Subject);
                context.UserData.SetValue<string>("RainbirdLastRelationship", r.Question.Relationship);
                if(r.Question.Type.Contains("First"))
                    context.UserData.SetValue<bool>("RainbirdLastAnswerRequired", true);

                if (r.Question.DataType == "number")
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
                else if(r.Question.Concepts != null)
                {
                 
                    context.UserData.SetValue<string>("RainbirdLastObject", r.Question.Concepts[0].ConceptType);

                    HeroCard hc = new HeroCard();
                    hc.Text = r.Question.Prompt;
                    List<CardAction> cardButtons = new List<CardAction>();

                    foreach (Concept x in r.Question.Concepts)
                    {
                        CardAction ca = new CardAction("imBack", x.Name, null, x.Value);
                        cardButtons.Add(ca);
                    }
                    hc.Buttons = cardButtons;
                    message.Attachments.Add(hc.ToAttachment());
                }
                else
                {
                    // Now a string without concepts
                    message.Text = r.Question.Prompt;
                }

            }

            if (result.Contains("result"))
            {
                RainBirdResultResponse r = JsonConvert.DeserializeObject<RainBirdResultResponse>(result);
                string resultString;
                message.Text = "";
                foreach (RainBirdResult rr in r.Results)
                {
                    resultString = $"{rr.Relationship} with {rr.Certainty}% confidence: {rr.Object} \n\r {rr.ObjectMetadata.en[0].Data} \n\r\n\r[Why Analysis](http://microsoft.com): {rr.FactId}\n\r";
                    message.Text = message.Text + resultString;
                }

                if (result == "")
                {
                    message.Text = "No recommendation found.";
                }
            }
            
            if (result.Contains("exception"))
            {
                RainBirdExceptionResponse r = JsonConvert.DeserializeObject<RainBirdExceptionResponse>(result);
                message.Text = r.Exception;
            }

            return message;

        }

        private string RainBirdProperty(string key, ref IDialogContext context)
        {
            string outStr = "";

            if (context.UserData.TryGetValue(key, out outStr))
            {
                return outStr;
            }

            return "";
        }
    }
}