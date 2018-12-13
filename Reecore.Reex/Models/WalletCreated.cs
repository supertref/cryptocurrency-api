using System;

namespace Reecore.Reex.Models
{
    public class WalletCreated
    {
        #region constructor
        /// <summary>
        /// Initialise a response for a newly created wallet
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="myAddress"></param>
        /// <param name="label"></param>
        public WalletCreated(Guid Id, string myAddress, string label)
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
