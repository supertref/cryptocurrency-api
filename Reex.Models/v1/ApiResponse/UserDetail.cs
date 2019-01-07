using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class UserDetail
    {
        #region constructors
        public UserDetail(string userId, string email, bool isVerfied, bool isMfaEnabled)
        {
            this.UserId = userId;
            this.Email = email;
            this.IsVerified = isVerfied;
            this.IsMfaEnabled = isMfaEnabled;
        }
        #endregion

        #region properties
        [JsonProperty("id")]
        public string UserId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("isVerified")]
        public bool IsVerified { get; set; }
        [JsonProperty("isMfaEnabled")]
        public bool IsMfaEnabled { get; set; }
        #endregion
    }
}
