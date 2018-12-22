using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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

        #region constructors
        public CosmosDbService(IConfiguration configuration)
        {
            this.databaseId = configuration["DatabaseId"];
            this.collectionId = configuration["CollectionId"];
            this.documentDbEndpoint = new Uri(configuration["DbEndpoint"]);
            this.documentDbMasterKey = configuration["MasterKey"];
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
                
                return wallet.First();
            }
        }

        public async Task<IList<Wallet>> GetWallets(Guid userId)
        {
            using (var client = new DocumentClient(documentDbEndpoint, documentDbMasterKey,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Gateway, ConnectionProtocol = Protocol.Https }))
            {
                var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                var wallets = await client.CreateDocumentQuery<Wallet>(collectionUri)
                    .Where(x => x.UserId == userId)
                    .AsDocumentQuery()
                    .ExecuteNextAsync<Wallet>();

                return wallets.ToList();
            }
        }
        #endregion
    }
}
