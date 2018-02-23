using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace DirectLineSampleClient
{
    public static class SpeechRecognition
    {
        static MicrophoneRecognitionClient micClient;
        static String finaltext = "";
        static bool flag = true;
        static bool timerStarted = false;
        static DateTime previousTime;

        static DirectLineClient mClient;
        static Microsoft.Bot.Connector.DirectLine.Conversation mConversation;

        public delegate void SendRequestToBotDelegate(string input, DirectLineClient client, Microsoft.Bot.Connector.DirectLine.Conversation conversation);

        private static SendRequestToBotDelegate functionPointer;

        static SpeechRecognition()
        {
            CreateMicrophoneRecoClient();
        }

        static string GetAuthenticationUri()
        {
            return "";
        }

        static void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Console.WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
            Console.WriteLine("********* Microphone status: {0} *********", e.Recording);
            if (e.Recording)
            {
                Console.WriteLine("Please start speaking.");
            }

            Console.WriteLine();
        }

        static void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine("{0}", e.PartialResult);
            finaltext = e.PartialResult;
            previousTime = DateTime.Now;
            if (!timerStarted)
            {
                timerStarted = true;
            }
            previousTime = DateTime.Now;

        }

        static void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            //Console.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

            // we got the final result, so it we can end the mic reco.  No need to do this
            // for dataReco, since we already called endAudio() on it as soon as we were done
            // sending all the data.
            finaltext = e.PhraseResponse.Results[0].DisplayText;
            //Console.WriteLine(finaltext);
            flag = false;
        }

        static void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            Console.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            Console.WriteLine("Error text: {0}", e.SpeechErrorText);
            Console.WriteLine();
        }
        static void CreateMicrophoneRecoClient()
        {
            micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.ShortPhrase,
                "en-Us",
                "b43e6e21b35b47ada3dce89cf362605e");
            micClient.AuthenticationUri = GetAuthenticationUri();

            // Event handlers for speech recognition results
            micClient.OnMicrophoneStatus += OnMicrophoneStatus;
            micClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            micClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            micClient.OnConversationError += OnConversationErrorHandler;
        }

        private static async Task SendMessageToBotAsync()
        {
            while(true)
            {
                if(timerStarted)
                {
                    DateTime curTime = DateTime.Now;
                    TimeSpan span = curTime - previousTime;
                    int ms = (int)span.TotalMilliseconds;
                    if (ms > 500)
                    {
                        functionPointer(finaltext, mClient, mConversation);
                        break;
                    }
                }
                await Task.Delay(200);
            }
        }

        public static string start(DirectLineClient client, Microsoft.Bot.Connector.DirectLine.Conversation conversation, SendRequestToBotDelegate funpointer)
        {
            timerStarted = false;
            flag = true;
            mClient = client;
            mConversation = conversation;
            functionPointer = funpointer;

            System.Threading.Thread t2 = new System.Threading.Thread(async () => await SendMessageToBotAsync());

            t2.Start();


            micClient.StartMicAndRecognition();

            t2.Join();

            while (flag) ;

            micClient.EndMicAndRecognition();
            micClient.Dispose();


            return finaltext;
        }
    }
}
