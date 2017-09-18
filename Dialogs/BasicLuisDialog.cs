using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand";
            await context.PostAsync(message);
    
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("greetings")]
        public async Task GreetingsIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Hi there, what can I do for you?");
            context.Wait(MessageReceived);
        }
        
        [LuisIntent("Info.General")]
        public async Task GeneralInfoIntent(IDialogContext context, LuisResult result)
        {
            string replyMessage;
            string entity;
            if (TryFindEntity(result, "Info.Keyword", out entity))
            {
    
                switch (entity.ToLowerInvariant())
                {
                    case "yourself":
                        replyMessage = "My name is Louis, I'll try to demonstrate the power of LUIS.ai";
                        break;
                    case "microsoft":
                        replyMessage = "Microsoft is a big company that makes computer software and video games for users around the world. Bill Gates and Paul Allen founded the company in 1975";
                        break;
                    case "bill gates":
                        replyMessage = "Bill Gates is a co-founder of the Microsoft Corporation.";
                        break;
                    case "arthur":
                    case "Asir":
                    case "awesome":
                        replyMessage = "Yes, Arthur is Windows phone fantastics!";
                        break;
                    default:
                        //replyMessage = $"Sorry, I have no information for {entity}";
                        replyMessage = $"Yes, I love {entity}";
                        break;
                }
            }
            else
            {
                replyMessage = "Sorry, no information!";
            }
    
            await context.SayAsync(text: replyMessage, speak: replyMessage);
            context.Wait(MessageReceived);
        }
        
        [LuisIntent("Music.Play")]
        public async Task PlayMusicIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Your intent: Music.Play");
            context.Wait(MessageReceived);
        }
    }
}