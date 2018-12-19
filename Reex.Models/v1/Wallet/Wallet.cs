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
        /// <summary>
        /// Initialise a new Wallet
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <param name="userId">Guid</param>
        /// <param name="privateKey">string</param>
        /// <param name="mnemo">string</param>
        /// <param name="isEncrypted">bool</param>
        /// <param name="label">string</param>
        /// <param name="email">string</param>
        /// <param name="addresses">IList<Address></param>
        public Wallet(Guid Id, Guid userId, string privateKey, string mnemo, bool isEncrypted, string label, string email, IList<Address> addresses)
            : base(SUCCESS, null)
        {
            this.ID = Id;
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
        public Guid ID { get; }
        public Guid UserId { get; }
        public string PrivateKey { get; }
        public string MnemonicCode { get; }
        public bool IsEncrypted { get; }
        public string Label { get; }
        public string Email { get; }
        public IList<Address> Addresses { get; }
        #endregion
    }
}
