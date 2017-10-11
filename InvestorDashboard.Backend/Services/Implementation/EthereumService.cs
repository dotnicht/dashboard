using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Signer;
using System;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : IEthereumService
    {
        private readonly IOptions<Ethereum> _options;
        private readonly IKeyVaultService _keyVaultService;

        public EthereumService(IOptions<Ethereum> options, IKeyVaultService keyVaultService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
    }
}
