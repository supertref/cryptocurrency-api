using System;

namespace Reecore.Reex.Models
{
    /// <summary>
    /// Defines an address associated to a wallet
    /// </summary>
    public class Address
    {
        #region constructors
        /// <summary>
        /// Initialise a new Address
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <param name="walletId">Guid</param>
        /// <param name="myAddress">string</param>
        /// <param name="label">string</param>
        public Address(Guid Id, Guid walletId, string myAddress, string label)
        {
            if(string.IsNullOrWhiteSpace(myAddress))
            {
                throw new ArgumentNullException(nameof(myAddress));
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            this.ID = Id;
            this.WalletId = walletId;
            this.MyAddress = myAddress;
            this.Label = label;
        }
        #endregion

        #region properties
        public Guid ID { get; }
        public Guid WalletId { get; }
        public string MyAddress { get; }
        public string Label { get; }
        #endregion
    }
}
