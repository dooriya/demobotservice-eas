using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        private static readonly string CurrentWeatherReplyTemplate = "Hi, It's {0} in {1} with temprature {2} deg c.";

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["LuisAppId"], ConfigurationManager.AppSettings["LuisAPIKey"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            string message;

            // For Julia's demo
            if (result.Query.ToLower().Contains("julia") || result.Query.ToLower().StartsWith("juli"))
            {
                message = "Julia Liuson is the Corporate Vice President of Visual Studio at Microsoft Corporation. And she oversees business and software development for Visual Studio and the .NET Framework";
            }
            else
            {
                message = $"Sorry I did not understand";
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "MyIntent" with the name of your newly created intent in the following handler
        [LuisIntent("greetings")]
        public async Task GreetingsIntent(IDialogContext context, LuisResult result)
        {
            if (result.Query.ToLower().Contains("bye"))
            {
                await context.PostAsync($"Good bye, dear");
            }
            else if (result.Query.ToLower().Contains("julia") || result.Query.ToLower().StartsWith("juli"))
            {
                await context.PostAsync("Julia Liuson is the Corporate Vice President of Visual Studio at Microsoft Corporation. And she oversees business and software development for Visual Studio and the .NET Framework");
            }
            else
            {
                await context.PostAsync($"Hi there, what can I do for you?");
            }

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
                        replyMessage = "My name is Louis, I'll try to demonstrate the power of LUIS.ai and Microsoft Bot Framework.";
                        break;
                    case "microsoft":
                        replyMessage = "Microsoft is a big company that makes computer software and video games for users around the world. And it is founded in 1975 by Bill Gates and Paul Allen. As of 2016, it is the world's largest software maker by revenue, and one of the world's most valuable companies.";
                        break;
                    case "bill gates":
                        replyMessage = "Bill Gates is a co-founder of the Microsoft Corporation.";
                        break;
                    case "julia":
                        replyMessage = "Julia Liuson is the Corporate Vice President of Visual Studio at Microsoft Corporation. And she oversees business and software development for Visual Studio and the .NET Framework";
                        break;
                    case "arthur":
                        replyMessage = "Arthur is Windows phone fantastics! And he is very rich!";
                        break;
                    default:
                        replyMessage = $"Sorry, I have no information for {entity}";
                        //replyMessage = $"Yes, I love {entity} very much.";
                        break;
                }
            }
            else
            {
                replyMessage = "Sorry, no information!";
            }

            // For Julia's demo
            if (result.Query.ToLower().Contains("julia"))
            {
                replyMessage = "Julia Liuson is the Corporate Vice President of Visual Studio at Microsoft Corporation. And she oversees business and software development for Visual Studio and the .NET Framework";
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

        [LuisIntent("Weather.GetForecast")]
        public async Task GetWeatherForecastIntent(IDialogContext context, LuisResult result)
        {
            string city;
            if (TryFindEntity(result, "Weather.Location", out city))
            {
                var weatherResponse = await this.GetCurrentWeatherByCityName(city);
                string replyMessage = string.Format(CurrentWeatherReplyTemplate,
                    weatherResponse.Summary,
                    weatherResponse.City,
                    weatherResponse.Temp);

                await context.SayAsync(text: replyMessage, speak: replyMessage);

            }
            else
            {
                await context.SayAsync(text: "Sorry, no information!", speak: "Sorry, no information!");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("GetDateTime")]
        public async Task GetDateTimeIntent(IDialogContext context, LuisResult result)
        {
            string dateTimeType;
            string dateTimeEntity;
            TryFindEntity(result, "DateTime.Type", out dateTimeType);
            TryFindEntity(result, "datetimeV2", out dateTimeEntity);

            string replyMessage;
            DateTime dateTime = DateTime.Now;
            switch (dateTimeType)
            {
                case "time":
                    replyMessage = $"It is {dateTime.TimeOfDay} now";
                    break;
                case "date":
                    replyMessage = $"It is {dateTime.Date} today";
                    break;
                case "day":
                    replyMessage = $"It is {dateTime.DayOfWeek} today";
                    break;
                default:
                    replyMessage = $"It is {dateTime.TimeOfDay} now";
                    break;
            }

            await context.SayAsync(text: replyMessage, speak: replyMessage);
            context.Wait(MessageReceived);
        }

        private bool TryFindEntity(LuisResult result, string entityType, out string entityValue)
        {
            EntityRecommendation entity;
            if (result.TryFindEntity(entityType, out entity))
            {
                entityValue = entity.Entity;
                return true;
            }
            else
            {
                entityValue = string.Empty;
                return false;
            }
        }

        private async Task<GetCurrentWeatherResponse> GetCurrentWeatherByCityName(string city)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("http://api.openweathermap.org");
                    HttpResponseMessage response = await client.GetAsync($"/data/2.5/weather?q={city}&units=metric&APPID=d01499238c7a7f3173938f86b7ad1fc8");
                    response.EnsureSuccessStatusCode();

                    var stringResult = await response.Content.ReadAsStringAsync();

                    var rawWeather = JsonConvert.DeserializeObject<OpenWeatherResponse>(stringResult);
                    string summary = string.Join(",", rawWeather.Weather.Select(x => x.Main));

                    return new GetCurrentWeatherResponse
                    {
                        Temp = rawWeather.Main.Temp,
                        Summary = string.Join(",", rawWeather.Weather.Select(x => x.Main)),
                        City = rawWeather.Name
                    };
                }
                catch (HttpRequestException httpRequestException)
                {
                    Console.WriteLine($"Error getting weather from OpenWeather: {httpRequestException.Message}");
                    throw;
                }
            }
        }
    }
}
