using Newtonsoft.Json;

namespace Reex.Models.v1.ApiResponse
{
    public class LoginStatus
    {
        #region constructors
        public LoginStatus(string userId, string email, bool isVerfied, int expiresIn, string firebaseToken, string refreshToken)
        {
            this.UserId = userId;
            this.Email = email;
            this.IsVerified = isVerfied;
            this.ExpiresIn = expiresIn;
            this.FirebaseToken = firebaseToken;
            this.RefreshToken = refreshToken;
        }
        #endregion

        #region properties
        [JsonProperty("id")]
        public string UserId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("isVerified")]
        public bool IsVerified { get; set; }
        [JsonProperty("token")]
        public string FirebaseToken { get; set; }
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }
        #endregion
    }
}
