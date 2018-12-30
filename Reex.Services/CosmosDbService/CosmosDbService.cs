using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Reex.Models.v1.Wallet;

namespace Reex.Services.CosmosDbService
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly string databaseId;
        private readonly string collectionId;
        private readonly Uri documentDbEndpoint;
        private readonly string documentDbMasterKey;
        private readonly string utcDate = DateTime.UtcNow.ToString("r");
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;

        #region constructors
        public CosmosDbService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this.databaseId = configuration["DatabaseId"];
            this.collectionId = configuration["CollectionId"];
            this.documentDbEndpoint = new Uri(configuration["DbEndpoint"]);
            this.documentDbMasterKey = configuration["MasterKey"];
            this.memoryCache = memoryCache;
            this.cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(int.Parse(configuration["CacheExpiryInMinutes"] ?? "60")));
        }
        #endregion

        #region public methods
        public async Task<Wallet> CreateWallet(Wallet wallet)
        {
            using (var client = new DocumentClient(documentDbEndpoint, documentDbMasterKey,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https }))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
                await client.CreateDocumentAsync(collectionUri, wallet);

                return wallet;
            }
        }

        public async Task<Wallet> UpdateWallet(Wallet wallet)
        {
            using (var client = new DocumentClient(documentDbEndpoint, documentDbMasterKey,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https }))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
                await client.UpsertDocumentAsync(collectionUri, wallet);

                return wallet;
            }
        }

        public async Task<Wallet> GetWallet(Guid walletId)
        {
            Wallet cachedWallet;
            var cacheKey = $"wallet-{walletId.ToString()}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedWallet);

            if (!doesExists)
            {
                using (var client = new DocumentClient(documentDbEndpoint, documentDbMasterKey,
                    new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https }))
                {
                    var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
                    var wallet = await client.CreateDocumentQuery<Wallet>(collectionUri, new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    })
                    .Where(x => x.WalletId == walletId)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<Wallet>();

                    var retunedWallet = wallet.First();

                    if(retunedWallet != null)
                    {
                        memoryCache.Set(cacheKey, retunedWallet, cacheEntryOptions);
                    }

                    return retunedWallet;
                }
            }

            return cachedWallet;
        }

        public async Task<IList<Wallet>> GetWallets(Guid userId)
        {
            List<Wallet> cachedWallets;
            var cacheKey = $"wallets-{userId.ToString()}";
            bool doesExists = memoryCache.TryGetValue(cacheKey, out cachedWallets);

            if (!doesExists)
            {
                using (var client = new DocumentClient(documentDbEndpoint, documentDbMasterKey,
                    new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https }))
                {
                    var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                    var wallets = await client.CreateDocumentQuery<Wallet>(collectionUri, new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    })
                    .Where(x => x.UserId == userId)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<Wallet>();

                    var returnedWallets = wallets.ToList();

                    if(returnedWallets.Any())
                    {
                        memoryCache.Set(cacheKey, returnedWallets, cacheEntryOptions);
                    }

                    return returnedWallets;
                }
            }

            return cachedWallets;
        }
        #endregion
    }
}
