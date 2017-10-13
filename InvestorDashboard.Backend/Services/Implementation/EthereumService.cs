using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Signer;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : IEthereumService
    {
        private readonly IOptions<Ethereum> _options;
        private readonly IMapper _mapper;
        private readonly IKeyVaultService _keyVaultService;

        public EthereumService(IOptions<Ethereum> options, IMapper mapper, IKeyVaultService keyVaultService)
        {
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

        public EthereumTransaction[] GetTransactionsByRecepientAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var result = RestUtil.Get<EtherscanResponse>($"{_options.Value.ApiUri}account/{address}/tx/0");

            if (result.Result != null)
            {
                return _mapper.Map<EthereumTransaction[]>(result.Result.Data.ToArray());
            }

            throw new InvalidOperationException("An error occurred while retrieving transaction list.");
        }

        internal class EtherscanResponse
        {
            public int Status { get; set; }
            public List<EtherscanTransaction> Data { get; set; }

            public class EtherscanTransaction
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
    }
}
