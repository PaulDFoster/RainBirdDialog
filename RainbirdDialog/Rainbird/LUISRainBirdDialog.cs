using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Threading;

namespace RainbirdDialog
{

    [LuisModel("7a4bcbd7-4c1f-47cf-98a1-2d6e6415c271", "0133a17b7e0a47f38f4e47c842e9ec64")]
    [Serializable]
    public class LUISRainBirdDialog : LuisDialog<object>
    {

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            // Cascade to QNAMaker Dialog
            // or
            // just bail with no comprendi message
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {

            await context.PostAsync("Hi! Try asking me things like 'what bank account should I have'");

            context.Wait(this.MessageReceived);
        }

        // LUIS Intent match to Rainbird query
        // Forward conversation to the RainBird query dialog
        
        [LuisIntent("BankAccount")]
        public async Task BankAccount(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            // Set RainBird session id to nothing to trigger fresh query 
            context.UserData.SetValue<string>("RainbirdSessionId", "");

            var rainbirdDialog = new RainbirdDialog("fe08d53b-189d-4bac-8a15-8aced896b1b5", "{\"subject\":\"the customer\",\"relationship\":\"recommended\"}");
            var messageToForward = await message;

            await context.Forward(rainbirdDialog, RainbirdDialogComplete, messageToForward, CancellationToken.None);

        }

        private async Task RainbirdDialogComplete(IDialogContext context, IAwaitable<bool> result)
        {
            var messageHandled = await result;
            if (!(bool)messageHandled)
            {
                await context.PostAsync("sorry message");
            }
            context.Wait(MessageReceived);
        }
    }

}