using Newtonsoft.Json;
using Reex.Models.v1.ApiResponse;
using System;
using System.Collections.Generic;

namespace Reex.Models.v1.Wallet
{
    /// <summary>
    /// Defines a users wallet and addresses associated with it.
    /// </summary>
    public class Wallet : RequestResponse
    {
        #region constructors
        public Wallet() : base(SUCCESS, null) { }
        /// <summary>
        /// Initialise a new Wallet
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <param name="userId">string</param>
        /// <param name="privateKey">string</param>
        /// <param name="mnemo">string</param>
        /// <param name="isEncrypted">bool</param>
        /// <param name="label">string</param>
        /// <param name="email">string</param>
        /// <param name="addresses">IList<Address></param>
        public Wallet(Guid Id, string userId, string privateKey, string mnemo, bool isEncrypted, string label, string email, IList<Address> addresses)
            : base(SUCCESS, null)
        {
            this.WalletId = Id;
            this.UserId = userId;
            this.PrivateKey = privateKey;
            this.MnemonicCode = mnemo;
            this.IsEncrypted = isEncrypted;
            this.Label = label;
            this.Email = email;
            this.Addresses = addresses;
        }
        #endregion

        #region properties
        [JsonProperty("walletId")]
        public Guid WalletId { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("privateKey")]
        public string PrivateKey { get; set; }
        [JsonProperty("mnemonicCode")]
        public string MnemonicCode { get; set; }
        [JsonProperty("isEncrypted")]
        public bool IsEncrypted { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("addresses")]
        public IList<Address> Addresses { get; set; }
        #endregion
    }
}
