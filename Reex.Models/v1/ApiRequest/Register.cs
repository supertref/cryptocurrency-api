using Newtonsoft.Json;

namespace Reex.Models.v1.ApiRequest
{
    public class Register
    {
        #region properties
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        #endregion
    }
}
