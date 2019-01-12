using Newtonsoft.Json;

namespace Reex.Models.v1.Wallet
{
    public class ExportPrivateKey
    {
        #region constructors
        public ExportPrivateKey(string privateKey, string mainAddress)
        {
            this.PrivateKey = privateKey;
            this.MainAddress = mainAddress;
        }
        #endregion

        #region properties
        [JsonProperty("privateKey")]
        public string PrivateKey { get; set; }
        [JsonProperty("mainAddress")]
        public string MainAddress { get; set; }
        #endregion
    }
}
