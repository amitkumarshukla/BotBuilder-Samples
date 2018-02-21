using System;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace DirectLineSampleClient
{
    public class SpeechRecognition
    {
        static MicrophoneRecognitionClient micClient;
        static String finaltext = "";
        static bool flag = true;
        static bool timerStarted = false;
        static DateTime previousTime;

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
            //Console.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Console.WriteLine("{0}", e.PartialResult);
            //Console.WriteLine();
            finaltext = e.PartialResult;
            if (!timerStarted)
            {
                timerStarted = true;
                previousTime = DateTime.Now;
            }
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
        public static string start()
        {
            flag = true;
            CreateMicrophoneRecoClient();
            micClient.StartMicAndRecognition();

            while (flag) ;

            micClient.EndMicAndRecognition();
            micClient.Dispose();


            return finaltext;
        }
    }
}
