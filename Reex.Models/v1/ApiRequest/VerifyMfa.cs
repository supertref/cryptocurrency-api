using Newtonsoft.Json;

namespace Reex.Models.v1.ApiRequest
{
    public class VerifyMfa
    {
        [JsonProperty("code")]
        public string MfaCode { get; set; }
    }
}
