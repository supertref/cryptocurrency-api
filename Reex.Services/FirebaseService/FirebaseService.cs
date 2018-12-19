using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Reex.Models.v1.Wallet;

namespace Reex.Services.FirebaseService
{
    // TODO resolve data query issue
    // https://github.com/ziyasal/FireSharp/
    public class FirebaseService : IFirebaseService
    {
        private const string WALLET_DOC = "wallets";

        private readonly IFirebaseConfig config;
        public IFirebaseClient client;

        #region constructors
        public FirebaseService(IConfiguration configuration)
        {
            config = new FirebaseConfig
            {
                AuthSecret = configuration["FirebaseAuthSecret"],
                BasePath = configuration["FirebaseBasePath"]
            };

            client = new FirebaseClient(config);
        }
        #endregion

        #region public methods
        public async Task<Wallet> CreateWallet(Wallet wallet)
        {
            var response = await client.PushTaskAsync($"{WALLET_DOC}", wallet);
            if(response.Exception != null)
            {
                throw new Exception("Error creating wallet", response.Exception);
            }
            return wallet;
        }

        // TODO resolve update wallet
        public async Task<Wallet> UpdateWallet(Wallet wallet)
        {
            var response = await client.PushTaskAsync($"{WALLET_DOC}", wallet);
            if (response.Exception != null)
            {
                throw new Exception("Error updating wallet", response.Exception);
            }
            return wallet;
        }

        public async Task<Wallet> GetWallet(Guid walletId)
        {
            var response = await client.GetTaskAsync($"{WALLET_DOC}");
            if (response.Exception != null)
            {
                throw new Exception("Error getting wallet data", response.Exception);
            }
            var result = JsonConvert.DeserializeObject<Dictionary<string, Wallet>>(response.Body);

            return result
                .Where(x => x.Value.ID == walletId)
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        public async Task<IList<Wallet>> GetWallets(Guid userId)
        {
            var response = await client.GetTaskAsync($"{WALLET_DOC}");
            if (response.Exception != null)
            {
                throw new Exception("Error getting wallets data", response.Exception);
            }
            var result = JsonConvert.DeserializeObject<Dictionary<string, Wallet>>(response.Body);

            return result
                .Where(x => x.Value.UserId == userId)
                .Select(x => x.Value)
                .ToList();
        }
        #endregion
    }
}
