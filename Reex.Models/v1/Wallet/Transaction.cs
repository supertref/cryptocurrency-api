using Newtonsoft.Json;
using System;

namespace Reex.Models.v1.Wallet
{
    public class Transaction
    {
        #region properties
        [JsonProperty("key")]
        public string Key => Guid.NewGuid().ToString().ToLower();
        [JsonProperty("account")]
        public string Account { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("vout")]
        public int Vout { get; set; }
        [JsonProperty("fee")]
        public decimal Fee { get; set; }
        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }
        [JsonProperty("bcconfirmations")]
        public int Bcconfirmations { get; set; }
        [JsonProperty("generated")]
        public string Generated { get; set; }
        [JsonProperty("txid")]
        public string TransactionId { get; set; }
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("timereceived")]
        public string Timereceived { get; set; }
        #endregion
    }
}
