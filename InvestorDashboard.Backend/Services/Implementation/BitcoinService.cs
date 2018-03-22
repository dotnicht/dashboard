using AutoMapper;
using Info.Blockchain.API.BlockExplorer;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : CryptoService, IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _bitcoinSettings;

        private Network Network
        {
            get
            {
                return _bitcoinSettings.Value.IsTestNet
                    ? Network.TestNet
                    : Network.Main;
            }
        }

        public BitcoinService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IRestService restService,
            ICalculationService calculationService,
            ITokenService tokenService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings, 
            IOptions<BitcoinSettings> bitcoinSettings)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, resourceService, restService, calculationService, tokenService, mapper, tokenSettings, bitcoinSettings)
        {
            _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));
        }

        protected override (string Address, string PrivateKey) GenerateKeys(string password = null)
        {
            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(Network).ToString();
            var encrypted = privateKey.GetEncryptedBitcoinSecret(password ?? KeyVaultService.InvestorKeyStoreEncryptionPassword, Network).ToString();
            return (Address: address, PrivateKey: encrypted);
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var be = new BlockExplorer();
            var addr = await be.GetBase58AddressAsync(address);

            var mapped = Mapper.Map<List<CryptoTransaction>>(addr.Transactions);

            foreach (var tx in addr.Transactions)
            {
                var result = mapped.Single(x => x.Hash == tx.Hash);

                if (tx.Inputs.All(x => x.PreviousOutput.Address == address))
                {
                    result.Amount = tx.Outputs.Where(x => x.Address != address).Sum(x => x.Value.Satoshis).ToString();
                    result.Direction = CryptoTransactionDirection.Internal;
                }
                else
                {
                    result.Amount = tx.Outputs.Where(x => x.Address == address).Sum(x => x.Value.Satoshis).ToString();
                    result.Direction = CryptoTransactionDirection.Inbound;
                }
            }

            return mapped;
        }

        protected override async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, BigInteger? amount = null)
        {
            // TODO: implement custom amount transfer.

            var secret = new BitcoinEncryptedSecretNoEC(address.PrivateKey, Network).GetSecret(KeyVaultService.InvestorKeyStoreEncryptionPassword);
            var response = await RestService.GetAsync<EarnResponse>(new Uri("https://bitcoinfees.earn.com/api/v1/fees/recommended"));

            var balance = 0m;
            var transaction = new Transaction();

            foreach (var tx in (await new BlockExplorer().GetBase58AddressAsync(address.Address)).Transactions)
            {
                for (var i = 0; i < tx.Outputs.Count; i++)
                {
                    if (tx.Outputs[i].Address == address.Address && !tx.Outputs[i].Spent)
                    {
                        balance += tx.Outputs[i].Value.GetBtc();
                        transaction.AddInput(new TxIn
                        {
                            PrevOut = new OutPoint(new uint256(tx.Hash), i),
                            ScriptSig = secret.GetAddress().ScriptPubKey
                        });
                    }
                }
            }

            transaction.AddOutput(new TxOut
            {
                ScriptPubKey = BitcoinAddress.Create(destinationAddress, Network).ScriptPubKey
            });

            var fee = Money.Satoshis(response.FastestFee * transaction.GetVirtualSize());
            var value = Money.Coins(balance);

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                transaction.Outputs.Single().Value = adjustedAmount;
                transaction.Sign(secret, false);

                Logger.LogDebug($"Publishing transaction {transaction.ToHex()}");

                var node = Node.Connect(Network, Settings.Value.NodeAddress.ToString());
                node.VersionHandshake();
                node.SendMessage(new InvPayload(transaction));
                node.SendMessage(new TxPayload(transaction));
                await Task.Delay(TimeSpan.FromSeconds(5));
                node.Disconnect();

                return (Hash: transaction.GetHash().ToString(), AdjustedAmount: adjustedAmount.Satoshi, Success: true);
            }

            if (value > 0)
            {
                Logger.LogWarning($"Transaction publish failed. Address: {address.Address}. Value: {value}. Fee: {fee}.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
        }

        private class EarnResponse
        {
            public int FastestFee { get; set; }
            public int HalfHourFee { get; set; }
            public int HourFee { get; set; }
        }
    }
}
