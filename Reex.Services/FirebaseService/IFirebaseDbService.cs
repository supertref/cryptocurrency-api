using Reex.Models.v1.ApiResponse;
using Reex.Models.v1.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reex.Services.FirebaseService
{
    public interface IFirebaseDbService
    {
        #region wallet
        Task<Wallet> CreateWallet(Wallet wallet);
        Task<Wallet> UpdateWallet(string key, Wallet wallet);
        Task<Wallet> GetWallet(Guid walletId);
        Task<string> GetWalletKey(Guid walletId);
        Task<IList<Wallet>> GetWallets(string userId);
        #endregion

        #region user
        Task CreateUserProperties(UserProperties userProperties);
        Task UpdateUserProperties(string key, UserProperties userProperties);
        Task<UserProperties> GetUserProperties(string userId);
        Task<string> GetUserPropertiesKey(string userId);
        #endregion
    }
}
