using Newtonsoft.Json;
using Reex.Models.v1.ApiResponse;

namespace Reex.Models.v1.Wallet
{
    public class Balance : RequestResponse
    {
        #region constrcutors
        public Balance(decimal availableBalance, decimal confirmedBalance, string symbol, bool divisibility)
            : base(SUCCESS, null)
        {
            this.AvailableBalance = availableBalance;
            this.ConfirmedBalance = confirmedBalance;
            this.Symbol = symbol;
            this.Divisibility = divisibility;
        }
        #endregion

        #region properties
        [JsonProperty("available_balance")]
        public decimal AvailableBalance { get; }
        [JsonProperty("confirmedBalance")]
        public decimal ConfirmedBalance { get; }
        [JsonProperty("symbol")]
        public string Symbol { get; }
        [JsonProperty("divisibility")]
        public bool Divisibility { get; }
        #endregion
    }
}
