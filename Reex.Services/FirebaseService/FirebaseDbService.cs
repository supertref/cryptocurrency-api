using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Reex.Models.v1.ApiResponse;
using Reex.Models.v1.Wallet;

namespace Reex.Services.FirebaseService
{
    public class FirebaseDbService : IFirebaseDbService
    {
        #region fields
        private readonly FirebaseClient firebaseClient;
        private readonly string walletCollectionId;
        private readonly string userCollectionId;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        #endregion

        #region constructors
        public FirebaseDbService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.walletCollectionId = configuration["WalletCollectionId"];
            this.userCollectionId = configuration["UserCollectionId"];
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));

            this.firebaseClient = new FirebaseClient(
              configuration["FirebaseDatabase"],
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(configuration["FirebaseDbSecret"])
              });
        }
        #endregion

        #region public methods
        public async Task<Wallet> CreateWallet(Wallet wallet)
        {
            var result = await firebaseClient
                .Child(walletCollectionId)
                .PostAsync(JsonConvert.SerializeObject(wallet));

            return wallet;
        }

        public async Task CreateUserProperties(UserProperties userProperties)
        {
            var result = await firebaseClient
                .Child(userCollectionId)
                .PostAsync(JsonConvert.SerializeObject(userProperties));
        }

        public async Task UpdateUserProperties(string key, UserProperties userProperties)
        {
            await firebaseClient
                .Child(userCollectionId)
                .Child(key)
                .PutAsync<UserProperties>(userProperties);
        }

        public async Task<UserProperties> GetUserProperties(string userId)
        {
            UserProperties cachedUserProperties;
            var cacheKey = $"user-{userId}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedUserProperties);

            if (!doesExists)
            {
                var result = await firebaseClient
                    .Child(userCollectionId)
                    .OrderBy(nameof(userId))
                    .StartAt(userId.ToString())
                    .LimitToFirst(1)
                    .OnceAsync<UserProperties>();

                var returnedUser = result.Where(x => x.Object.UserId == userId).Select(x => x.Object).FirstOrDefault();

                if (returnedUser != null)
                {
                    memoryCache.Set(cacheKey, returnedUser, cacheEntryOptions);
                }

                return returnedUser;
            }

            return cachedUserProperties;
        }

        public async Task<string> GetUserPropertiesKey(string userId)
        {
            string cachedUserPropertiesKey;
            var cacheKey = $"user-key-{userId}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedUserPropertiesKey);

            if (!doesExists)
            {
                var result = await firebaseClient
                    .Child(userCollectionId)
                    .OrderBy(nameof(userId))
                    .StartAt(userId.ToString())
                    .LimitToFirst(1)
                    .OnceAsync<UserProperties>();

                var returnedUserKey = result.Where(x => x.Object.UserId == userId).Select(x => x.Key).FirstOrDefault();

                if (returnedUserKey != null)
                {
                    memoryCache.Set(cacheKey, returnedUserKey, cacheEntryOptions);
                }

                return returnedUserKey;
            }

            return cachedUserPropertiesKey;
        }

        public async Task<Wallet> GetWallet(Guid walletId)
        {
            Wallet cachedWallet;
            var cacheKey = $"wallet-{walletId.ToString()}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedWallet);

            if (!doesExists)
            {
                var result = await firebaseClient
                    .Child(walletCollectionId)
                    .OrderBy(nameof(walletId))
                    .StartAt(walletId.ToString())
                    .LimitToFirst(1)
                    .OnceAsync<Wallet>();

                var returnedWallet = result.Where(x => x.Object.WalletId == walletId).Select(x => x.Object).FirstOrDefault();

                if (returnedWallet != null)
                {
                    memoryCache.Set(cacheKey, returnedWallet, cacheEntryOptions);
                }

                return returnedWallet;
            }

            return cachedWallet;
        }

        public async Task<string> GetWalletKey(Guid walletId)
        {
            string key;
            var cacheKey = $"wallet-key-{walletId.ToString()}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out key);

            if (!doesExists)
            {
                var result = await firebaseClient
                    .Child(walletCollectionId)
                    .OrderBy(nameof(walletId))
                    .StartAt(walletId.ToString())
                    .LimitToFirst(1)
                    .OnceAsync<Wallet>();

                var returnedKey = result.Where(x => x.Object.WalletId == walletId).Select(x => x.Key).FirstOrDefault();

                if (!string.IsNullOrEmpty(returnedKey))
                {
                    memoryCache.Set(cacheKey, returnedKey, cacheEntryOptions);
                }

                return returnedKey;
            }

            return key;
        }

        public async Task<IList<Wallet>> GetWallets(string userId)
        {
            List<Wallet> cachedWallets;
            var cacheKey = $"wallets-{userId.ToString()}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedWallets);

            if (!doesExists)
            {
                var result = await firebaseClient
                    .Child(walletCollectionId)
                    .OrderBy(nameof(userId))
                    .StartAt(userId)
                    .LimitToFirst(1)
                    .OnceAsync<Wallet>();

                var returnedWallets = result.Where(x => x.Object.UserId == userId).Select(x => x.Object).ToList();

                if (returnedWallets.Any())
                {
                    memoryCache.Set(cacheKey, returnedWallets, cacheEntryOptions);
                }

                return returnedWallets;
            }

            return cachedWallets;
        }

        public async Task<Wallet> UpdateWallet(string key, Wallet wallet)
        {
            await firebaseClient
                .Child(walletCollectionId)
                .Child(key)
                .PutAsync(wallet);

            return wallet;
        }
        #endregion
    }
}
