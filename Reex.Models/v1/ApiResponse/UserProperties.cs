using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class UserProperties
    {
        #region constructors
        public UserProperties() { }

        public UserProperties(string userId, string secret, string issuer, string account)
        {
            this.UserId = userId;
            this.Secret = secret;
            this.Issuer = issuer;
            this.Account = account;
        }

        public UserProperties(string userId, string secret, string issuer, string account, bool isMfaEnabled)
        {
            this.UserId = userId;
            this.Secret = secret;
            this.Issuer = issuer;
            this.Account = account;
            this.IsMfaEnabled = IsMfaEnabled;
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
        [JsonProperty("issuer")]
        public string Issuer { get; set; }
        [JsonProperty("account")]
        public string Account { get; set; }
        #endregion
    }
}
