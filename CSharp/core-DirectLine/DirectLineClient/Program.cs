namespace DirectLineSampleClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
    using Models;
    using Newtonsoft.Json;

    public class Program
    {
        [Serializable]
        public class BotCurrentDialogState
        {
            public int CurState { get; set; }
            public BotCurrentDialogState(int curState)
            {
                CurState = curState;
            }
        }

        [Serializable]
        public class MyAppCard
        {
            public string AppName { get; set; }
            public string AppHandler { get; set; }
        }

        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotId"];
        private static string fromUser = "DirectLineSampleClientUser";
        private static int currentStateNumber = 0;

        public static void Main(string[] args)
        {
            StartBotConversation().Wait();
        }

        private static async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(directLineSecret);

            var conversation = await client.Conversations.StartConversationAsync();

            System.Threading.Thread t1 = new System.Threading.Thread(async () => await ReadBotMessagesAsync(client, conversation.ConversationId));
            t1.Start();

            System.Threading.Thread t2 = new System.Threading.Thread(() => SendMessageToBotAsync(client, conversation));

            t2.Start();

            t2.Join();
            t1.Join();

        }



        private static void SendRequestToBot(string input, DirectLineClient client, Conversation conversation)
        {
            if (input.Trim().ToLower() == "exit" || input.Trim().ToLower() == "exit.")
            {
                return;
            }
            else
            {
                if (input.Length > 0)
                {

                    Activity userMessage = new Activity
                    {
                        From = new ChannelAccount(fromUser),
                        Text = input,
                        Type = ActivityTypes.Message
                    };

                    client.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
                }
            }
        }

        private static void SendMessageToBotAsync(DirectLineClient client, Conversation conversation)
        {
            SpeechRecognition.SendRequestToBotDelegate funpointer = SendRequestToBot;
            SpeechRecognition speechreco = new SpeechRecognition(client, conversation, funpointer);

            while (true)
            {
                Console.Write("Enter and than speak> ");
                string input = Console.ReadLine().Trim();

                speechreco.start();
            }
        }

        private static async Task ReadBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            while (true)
            {
                var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activitySet?.Watermark;

                var activities = from x in activitySet.Activities
                                 where x.From.Id == botId
                                 select x;

                foreach (Activity activity in activities)
                {
                    Console.WriteLine(activity.Text);

                    if (activity.Attachments != null)
                    {
                        foreach (Attachment attachment in activity.Attachments)
                        {
                            switch (attachment.ContentType)
                            {
                                case "application/vnd.microsoft.card.hero":
                                    RenderHeroCard(attachment);
                                    break;

                                case "image/png":
                                    Console.WriteLine($"Opening the requested image '{attachment.ContentUrl}'");
                                    Process.Start(attachment.ContentUrl);
                                    break;

                                case "application/json":
                                    MyAppCard account = JsonConvert.DeserializeObject<MyAppCard>(attachment.Content.ToString());

                                    //Console.WriteLine(account.AppName + " :: " + account.AppHandler);
                                    int minDistance = 100;
                                    string key = "";
                                    foreach (var item in Hardcoded.appNamevsExeMapping)
                                    {
                                        int distance = LevenshteinDistance.Compute(account.AppName, item.Key);
                                        if (distance < minDistance)
                                        {
                                            minDistance = distance;
                                            key = item.Key;
                                        }
                                    }
                                    if (key != "")
                                    {
                                        TextToSpeechBot ttsbot = new TextToSpeechBot();
                                        System.Diagnostics.Process.Start(Hardcoded.appNamevsExeMapping[key]);
                                        ttsbot.MainTTS("I am opening " + account.AppName + " for you. Please enjoy it.");
                                    }
                                    break;
                            }
                        }
                    }

                    Console.Write("Command> ");
                }

                await Task.Delay(2000);

            }
        }

        private static void RenderHeroCard(Attachment attachment)
        {
            const int Width = 70;
            Func<string, string> contentLine = (content) => string.Format($"{{0, -{Width}}}", string.Format("{0," + ((Width + content.Length) / 2).ToString() + "}", content));

            var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());

            if (heroCard != null)
            {
                Console.WriteLine("/{0}", new string('*', Width + 1));
                Console.WriteLine("*{0}*", contentLine(heroCard.Title));
                Console.WriteLine("*{0}*", new string(' ', Width));
                Console.WriteLine("*{0}*", contentLine(heroCard.Text));
                Console.WriteLine("{0}/", new string('*', Width + 1));
            }
        }
    }
}
