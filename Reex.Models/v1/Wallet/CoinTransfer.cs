namespace Reex.Models.v1.Wallet
{
    public class CoinTransfer
    {
        #region constructors
        public CoinTransfer(string transactionId)
        {
            this.TransactionId = transactionId;
        }
        #endregion

        #region public methods
        public string TransactionId { get; set; }
        #endregion
    }
}
