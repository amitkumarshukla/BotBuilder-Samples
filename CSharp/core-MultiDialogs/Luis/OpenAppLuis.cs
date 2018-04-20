using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MultiDialogsBot.Luis
{
    public class Rootobject
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Topscoringintent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }
    public class OpenAppLuis
    {
        public static async Task<Rootobject> MakeRequest(string input)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // This app ID is for a public sample app that recognizes requests to turn on and turn off lights
            //var luisAppId = "d3497295-e2e6-4916-a290-24d034ffa71a";
            //var subscriptionKey = "2e69506f9cb8405d8fe74a13a11faa37";

            //Saumya keys
            var luisAppId = "7e9787b0-47e3-43f0-bc23-758f86813a56";
            var subscriptionKey = "5f6aca63c1fd43e6b6a8cd220cc7f3d3";

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = input;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";

            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<Rootobject>(strResponseContent.ToString()); 
        }
    }
}
