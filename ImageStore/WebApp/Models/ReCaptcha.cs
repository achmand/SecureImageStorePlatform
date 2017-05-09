using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApp.Models
{
    public sealed class ReCaptcha
    {
        public static string Validate(string encodedResponse)
        {
            var client = new System.Net.WebClient();
            const string privateKey = "6LeGiCAUAAAAAOFHytOplBSy43AO_OpWk68WU1lZ";

            var googleReply = client.DownloadString(
                $"https://www.google.com/recaptcha/api/siteverify?secret={privateKey}&response={encodedResponse}");

            var captchaResponse = JsonConvert.DeserializeObject<ReCaptcha>(googleReply);
            return captchaResponse.Success;
        }

        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}