using Reex.Models.v1.ApiResponse;
using System;

namespace Reex.Models.v1.Wallet
{
    public class WalletCreated : RequestResponse
    {
        #region constructor
        /// <summary>
        /// Initialise a response for a newly created wallet
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="myAddress"></param>
        /// <param name="label"></param>
        public WalletCreated(Guid Id, string myAddress, string label)
            : base(SUCCESS, null)
        {
            this.ID = Id;
            this.MyAddress = myAddress;
            this.Label = label;
        }
        #endregion

        #region properties
        public Guid ID { get; }
        public string MyAddress { get; }
        public string Label { get; }
        #endregion
    }
}
