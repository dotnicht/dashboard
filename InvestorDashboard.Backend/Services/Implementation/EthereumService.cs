using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : IEthereumService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<EthereumSettings> _options;
        private readonly IMapper _mapper;
        private readonly IKeyVaultService _keyVaultService;

        public EthereumService(ApplicationDbContext context, IOptions<EthereumSettings> options, IMapper mapper, IKeyVaultService keyVaultService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        }

        public EthereumAccount CreateAccount()
        {
            var ecKey = EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var keyStoreService = new KeyStorePbkdf2Service();
            var bytes = ecKey.GetPrivateKeyAsBytes();
            var address = ecKey.GetPublicAddress();
            var json = keyStoreService.EncryptAndGenerateKeyStoreAsJson(_keyVaultService.KeyStoreEncryptionPassword, bytes, address);

            return new EthereumAccount
            {
                Address = ecKey.GetPublicAddress(),
                KeyStore = json
            };
        }

        public IEnumerable<EthereumTransaction> GetInboundTransactionsByRecipientAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            return GetInboundTransactionsByRecipientAddressFromEtherscan(address);
        }

        private EthereumTransaction[] GetInboundTransactionsByRecipientAddressFromEtherchain(string address)
        {

            var result = RestUtil.Get<EtherchainResponse>($"{_options.Value.EtherchainApiUri}account/{address}/tx/0");

            if (result != null)
            {
                return _mapper.Map<EthereumTransaction[]>(result.Data.Where(x => x.Recipient == address && x.Type == "tx"));
            }

            throw new InvalidOperationException("An error occurred while retrieving transaction list from etherchain.org.");
        }

        private EthereumTransaction[] GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_options.Value.EtherscanApiUri}module=account&action=txlist&address={address}&startblock=0&endblock=99999999&sort=asc&apikey={_options.Value.EtherscanApiKey}";
            var result = RestUtil.Get<EtherscanResponse>(uri);

            if (result != null)
            {
                return _mapper.Map<EthereumTransaction[]>(result.Result.Where(x => x.To == address));
            }

            throw new InvalidOperationException("An error occurred while retrieving transaction list from etherscan.io.");
        }


        internal class EtherchainResponse
        {
            public int Status { get; set; }
            public List<Transaction> Data { get; set; }

            public class Transaction
            {
                public string Hash { get; set; }
                public string Sender { get; set; }
                public string Recipient { get; set; }
                public string AccountNonce { get; set; }
                public object Price { get; set; }
                public int GasLimit { get; set; }
                public decimal Amount { get; set; }
                public int Block_id { get; set; }
                public DateTime Time { get; set; }
                public int NewContract { get; set; }
                public object IsContractTx { get; set; }
                public string BlockHash { get; set; }
                public string ParentHash { get; set; }
                public int? TxIndex { get; set; }
                public int GasUsed { get; set; }
                public string Type { get; set; }
            }
        }

        internal class EtherscanResponse
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public List<Transaction> Result { get; set; }

            public class Transaction
            {
                public string BlockNumber { get; set; }
                public string BlockHash { get; set; }
                public string TimeStamp { get; set; }
                public string Hash { get; set; }
                public string Nonce { get; set; }
                public string TransactionIndex { get; set; }
                public string From { get; set; }
                public string To { get; set; }
                public string Value { get; set; }
                public string Gas { get; set; }
                public string GasPrice { get; set; }
                public string Input { get; set; }
                public string ContractAddress { get; set; }
                public string CumulativeGasUsed { get; set; }
                public string GasUsed { get; set; }
                public string Confirmations { get; set; }
                public string IsError { get; set; }
            }
        }
    }
}
