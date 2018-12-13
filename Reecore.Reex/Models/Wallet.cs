using System;
using System.Collections.Generic;

namespace Reecore.Reex.Models
{
    /// <summary>
    /// Defines a users wallet and addresses associated with it.
    /// </summary>
    public class Wallet
    {
        #region constructors
        /// <summary>
        /// Initialise a new Wallet
        /// </summary>
        /// <param name="Id">Guid</param>
        /// <param name="privateKey">string</param>
        /// <param name="isEncrypted">bool</param>
        /// <param name="label">string</param>
        /// <param name="email">string</param>
        /// <param name="addresses">IList<Address></param>
        public Wallet(Guid Id, string privateKey, bool isEncrypted, string label, string email, IList<Address> addresses)
        {
            this.ID = Id;
            this.PrivateKey = privateKey;
            this.IsEncrypted = isEncrypted;
            this.Label = label;
            this.Email = email;
            this.Addresses = addresses;
        }
        #endregion

        #region properties
        public Guid ID { get; }
        public string PrivateKey { get; }
        public bool IsEncrypted { get; }
        public string Label { get; }
        public string Email { get; }
        public IList<Address> Addresses { get; }
        #endregion
    }
}
