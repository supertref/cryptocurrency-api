using Reex.Models.v1.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reex.Services.FirebaseService
{
    public interface IFirebaseService
    {
        Task<Wallet> CreateWallet(Wallet wallet);
        Task<Wallet> UpdateWallet(Wallet wallet);
        Task<Wallet> GetWallet(Guid walletId);
        Task<IList<Wallet>> GetWallets(Guid userId);
    }
}
