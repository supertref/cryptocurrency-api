using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class UserProperties
    {
        #region constructors
        public UserProperties() { }

        public UserProperties(string userId, string secret)
        {
            this.UserId = userId;
            this.Secret = secret;
        }

        public UserProperties(string userId, string secret, bool isMfaEnabled)
        {
            this.UserId = userId;
            this.Secret = secret;
            this.IsMfaEnabled = isMfaEnabled;
        }
        #endregion

        #region properties
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("secret")]
        public string Secret { get; set; }
        [JsonProperty("isMfaEnabled")]
        public bool IsMfaEnabled { get; set; }
        #endregion
    }
}
