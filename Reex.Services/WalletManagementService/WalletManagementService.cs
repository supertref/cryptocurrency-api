using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client;
using Reex.Models.v1.ApiRequest;
using Reex.Models.v1.Wallet;
using Reex.Services.CosmosDbService;

namespace Reex.Services.WalletManagementService
{
    public class WalletManagementService : IWalletManagementService
    {
        #region fields
        private readonly Network network;
        private readonly ICosmosDbService cosmosDbService;
        #endregion

        #region constructors
        public WalletManagementService(NBitcoin.Altcoins.Reex instance, ICosmosDbService cosmosDbService)
        {
            network = instance.Mainnet;
            this.cosmosDbService = cosmosDbService;
        }
        #endregion

        #region public methods
        public async Task<IList<Wallet>> GetWallets(Guid id, string email)
        {
            var wallets = await cosmosDbService.GetWallets(id);
            return wallets;
        }

        public async Task<Wallet> GetWallet(Guid id, string email)
        {
            var wallet = await cosmosDbService.GetWallet(id);
            return wallet;
        }

        public async Task<IList<Address>> GetAddresses(Guid id)
        {
            var wallet = await cosmosDbService.GetWallet(id);
            return wallet.Addresses.Where(x => x.WalletId == id).ToList();
        }

        public async Task<Balance> GetBalance(Guid id, string email)
        {
            var wallet = await cosmosDbService.GetWallet(id);
            var reexUri = new Uri("http://123.139.175.122");

            try
            {
                var client = new QBitNinjaClient(reexUri, network);
                var balanceResult = await client.GetBalance(wallet.Addresses.FirstOrDefault().MyAddress, true);
                var available_balance = 0.0M;
                var confirmedBalance = 0.0M;

                if (balanceResult.Operations.Count > 0)
                {
                    var unspentCoins = new List<Coin>();
                    var unspentCoinsConfirmed = new List<Coin>();

                    foreach (var operation in balanceResult.Operations)
                    {
                        unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                        if (operation.Confirmations > 0)
                            unspentCoinsConfirmed.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
                    }

                    available_balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
                    confirmedBalance = unspentCoinsConfirmed.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
                }

                return new Balance(available_balance, confirmedBalance, "REEX", true);
            }
            catch(Exception ex)
            {
                var balance = new Balance(0, 0, "REEX", true);
                balance.Message = "An error occured when trying to get your balance";
                return balance;
            }
        }

        public async Task<WalletCreated> CreateWallet(CreateWallet request)
        {
            var walletId = Guid.NewGuid();
            var privateKey = new Key();
            var reexPrivateKey = privateKey.GetWif(network);
            var reexPublicKey = privateKey.PubKey.GetAddress(network);
            var walletLabel = "Main Wallet";
            var addressLabel = "Main Address";

            var addresses = new List<Address>()
            {
                new Address(Guid.NewGuid(), walletId, reexPublicKey.ToString(), addressLabel)
            };

            var wallet = new Wallet(walletId, request.Id, reexPrivateKey.ToString(), string.Empty, true, walletLabel, request.Email, addresses);
            var result = await cosmosDbService.CreateWallet(wallet);

            return new WalletCreated(wallet.WalletId, reexPublicKey.ToString(), addressLabel);
        }

        public async Task<Address> CreateAddress(CreateWalletAddress request)
        {
            var wallet = await cosmosDbService.GetWallet(request.Id);
            var privateKey = new BitcoinSecret(wallet.PrivateKey);
            var extKey = new ExtKey(privateKey.PubKey.ToHex());
            var newAddress = new Address(Guid.NewGuid(), wallet.WalletId, extKey.PrivateKey.PubKey.GetAddress(network).ToString(), request.Label);
            wallet.Addresses.Add(newAddress);
            var updateWallet = await cosmosDbService.UpdateWallet(wallet);

            return newAddress;
        }

        public async Task<CoinTransfer> SpendCoins(SpendCoins request)
        {
            var wallet = await cosmosDbService.GetWallet(request.Id);
            var privateKey = new BitcoinSecret(wallet.PrivateKey);
            var transaction = Transaction.Create(network);
            var fromAddress = privateKey.GetAddress().ToString();

            var addressBalanceResult = await this.GetBalance(request.Id, request.Email);

            decimal addressBalance = addressBalanceResult.AvailableBalance;
            decimal addressBalanceConfirmed = addressBalanceResult.ConfirmedBalance;

            if (addressBalanceConfirmed <= request.transferValue)
            {
                throw new Exception("The address doesn't have enough funds!");
            }

            var client = new QBitNinjaClient(network);
            var balance = await client.GetBalance(new BitcoinPubKeyAddress(fromAddress));

            // trx input
            // Get all transactions in for that address
            int txsIn = 0;
            if (balance.Operations.Count > 0)
            {
                var unspentCoins = new List<Coin>();
                foreach (var operation in balance.Operations)
                {
                    //string transaction = operation.TransactionId.ToString();

                    foreach (Coin receivedCoin in operation.ReceivedCoins)
                    {
                        OutPoint outpointToSpend = receivedCoin.Outpoint;
                        transaction.Inputs.Add(new TxIn() { PrevOut = outpointToSpend });
                        transaction.Inputs[txsIn].ScriptSig = privateKey.ScriptPubKey;
                        txsIn = txsIn + 1;
                    }
                }
            }

            // add address to send money
            var toPubKeyAddress = new BitcoinPubKeyAddress(request.ToAddress);
            var toAddressTxOut = new TxOut()
            {
                Value = new Money((decimal)request.transferValue, MoneyUnit.BTC),
                ScriptPubKey = toPubKeyAddress.ScriptPubKey
            };
            transaction.Outputs.Add(toAddressTxOut);

            // add address to send change
            Money change = 
                new Money((decimal)addressBalance, MoneyUnit.BTC) - new Money((decimal)request.transferValue, MoneyUnit.BTC) - GetMinerFee();

            if (change > 0)
            {
                var fromPubKeyAddress = new BitcoinPubKeyAddress(fromAddress);

                TxOut changeAddressTxOut = new TxOut()
                {
                    Value = change,
                    ScriptPubKey = fromPubKeyAddress.ScriptPubKey
                };
                transaction.Outputs.Add(changeAddressTxOut);
            }

            // sign transaction
            transaction.Sign(privateKey, false);

            // send transaction
            var broadcastResponse = await client.Broadcast(transaction);

            if (!broadcastResponse.Success)
            {
                throw new Exception("Error broadcasting transaction " + broadcastResponse.Error.ErrorCode + " : " + broadcastResponse.Error.Reason);
            }

            return new CoinTransfer(transaction.GetHash().ToString());
        }
        #endregion

        #region private methods
        private Money GetMinerFee()
        {
            var minerFee = new Money((decimal)0.0001, MoneyUnit.BTC);
            return minerFee;
        }
        #endregion
    }
}
