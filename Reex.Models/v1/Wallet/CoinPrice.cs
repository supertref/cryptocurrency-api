using Newtonsoft.Json;

namespace Reex.Models.v1.Wallet
{
    public class CoinPrice
    {
        [JsonProperty("last")]
        public decimal Price { get; set; }
        [JsonProperty("instrument")]
        public string Instrument { get; set; }
    }
}
