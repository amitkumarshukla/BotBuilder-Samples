using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace DirectLineSampleClient
{
    public class SpeechRecognition
    {
        private MicrophoneRecognitionClient micClient;
        private String finaltext = "";
        private bool flag = true;
        private bool timerStarted = false;
        private DateTime previousTime;

        private DirectLineClient mClient;
        private Microsoft.Bot.Connector.DirectLine.Conversation mConversation;

        public delegate void SendRequestToBotDelegate(string input, DirectLineClient client, Microsoft.Bot.Connector.DirectLine.Conversation conversation);

        private SendRequestToBotDelegate functionPointer;

        public SpeechRecognition(DirectLineClient client, Microsoft.Bot.Connector.DirectLine.Conversation conversation, SendRequestToBotDelegate funpointer)
        {
            CreateMicrophoneRecoClient();
            mClient = client;
            mConversation = conversation;
            functionPointer = funpointer;
        }

        private string GetAuthenticationUri()
        {
            return "";
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Console.WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
            Console.WriteLine("********* Microphone status: {0} *********", e.Recording);
            if (e.Recording)
            {
                Console.WriteLine("Please start speaking.");
            }

            Console.WriteLine();
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
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

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            //Console.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

            // we got the final result, so it we can end the mic reco.  No need to do this
            // for dataReco, since we already called endAudio() on it as soon as we were done
            // sending all the data.
            finaltext = e.PhraseResponse.Results[0].DisplayText;
            //Console.WriteLine(finaltext);
            flag = false;
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            Console.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            Console.WriteLine("Error text: {0}", e.SpeechErrorText);
            Console.WriteLine();
        }
        private void CreateMicrophoneRecoClient()
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

        private void SendMessageToBotAsync(Object obj)
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
                //await Task.Delay(200);
                Thread.Sleep(200);
            }
        }

        public string start()
        {
            timerStarted = false;
            flag = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(SendMessageToBotAsync), null);

            micClient.StartMicAndRecognition();

            while (flag) ;

            micClient.EndMicAndRecognition();
            //micClient.Dispose();


            return finaltext;
        }
    }
}
