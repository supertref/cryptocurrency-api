using Newtonsoft.Json;

namespace Reex.Models.v1.ApiRequest
{
    public class PasswordReset
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
