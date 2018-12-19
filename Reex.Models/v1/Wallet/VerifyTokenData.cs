using Newtonsoft.Json;
using System;

namespace Reex.Models.v1.Wallet
{
    public class VerifyTokenData
    {
        #region properties
        [JsonProperty("id")]
        public Guid ID { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("first_name")]
        public string Firstname { get; set; }
        [JsonProperty("last_name")]
        public string Lastname { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("company")]
        public string Company { get; set; }
        [JsonProperty("language")]
        public string Language { get; set; }
        #endregion
    }
}
