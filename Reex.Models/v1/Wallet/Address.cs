using Newtonsoft.Json;
using System;

namespace Reex.Models.v1.Wallet
{
    /// <summary>
    /// Defines an address associated to a wallet
    /// </summary>
    public class Address
    {
        #region constructors
        public Address() { }
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

            this.AddressId = Id;
            this.WalletId = walletId;
            this.MyAddress = myAddress;
            this.Label = label;
        }
        #endregion

        #region properties
        [JsonProperty("addressId")]
        public Guid AddressId { get; set; }
        [JsonProperty("walletId")]
        public Guid WalletId { get; set; }
        [JsonProperty("myAddress")]
        public string MyAddress { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        #endregion
    }
}
