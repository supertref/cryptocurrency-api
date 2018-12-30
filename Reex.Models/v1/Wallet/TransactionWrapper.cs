using Newtonsoft.Json;
using Reex.Models.v1.ApiResponse;
using System.Collections.Generic;

namespace Reex.Models.v1.Wallet
{
    public class TransactionWrapper : RequestResponse
    {
        #region constructors
        public TransactionWrapper() : base(SUCCESS, string.Empty)
        {
            this.Transactions = new List<Transaction>();
        }

        public TransactionWrapper(IList<Transaction> transactions) : base(SUCCESS, string.Empty)
        {
            this.Transactions = transactions;
        }
        #endregion

        #region properties
        [JsonProperty("data")]
        public IList<Transaction> Transactions { get; set; }
        #endregion
    }
}
