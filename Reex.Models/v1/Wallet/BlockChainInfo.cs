using Newtonsoft.Json;
using Reex.Models.v1.ApiResponse;

namespace Reex.Models.v1.Wallet
{
    public class BlockChainInfo : RequestResponse
    {
        #region constructors
        public BlockChainInfo() : base(SUCCESS, string.Empty) { }
        #endregion

        #region properties
        [JsonProperty("paytxfee")]
        public decimal PayTransactionFee { get; set; }
        [JsonProperty("relayfee")]
        public decimal RelayFee { get; set; }
        #endregion
    }
}
