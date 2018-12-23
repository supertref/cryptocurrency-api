using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.Wallet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reex.Services.WalletManagementService
{
    public interface IWalletManagementService
    {
        Task<Wallet> GetWallet(Guid id, string email);
        Task<IList<Wallet>> GetWallets(Guid id, string email);
        Task<IList<Address>> GetAddresses(Guid id);
        Task<Balance> GetBalance(Guid id, string email);
        Task<WalletCreated> CreateWallet(CreateWallet request);
        Task<Address> CreateAddress(CreateWalletAddress request);
        Task<CoinTransfer> SpendCoins(SpendCoins request);
    }
}
