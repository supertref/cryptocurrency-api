using Reex.Models.v1.ApiResponse;

namespace Reex.Models.v1.Wallet
{
    public class CoinTransfer : RequestResponse
    {
        #region constructors
        public CoinTransfer() : base(SUCCESS, string.Empty) { }

        public CoinTransfer(string transactionId, decimal relayFee) : base(SUCCESS, string.Empty)
        {
            this.TransactionId = transactionId;
            this.RelayFee = relayFee;
        }
        #endregion

        #region public methods
        public string TransactionId { get; set; }
        public decimal RelayFee { get; set; }
        #endregion
    }
}
