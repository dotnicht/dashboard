using InvestorDashboard.Business.ConfigurationSections;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Business.Services.Implementation
{
    internal class KeyVaultService : IKeyVaultService
    {
        private readonly IOptions<KeyVault> _options;
        private bool _initialized;

        public string DatabaseConnectionString { get; private set; }
        public string KeyStoreEncryptionPassword { get; private set; }

        public KeyVaultService(IOptions<KeyVault> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Initialize()
        {
            if (!_initialized)
            {
                using (var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken)))
                {
                    var connectionString = await keyVaultClient.GetSecretAsync(_options.Value.SecretUri, "DatabaseConnectionString").ConfigureAwait(false);
                    DatabaseConnectionString = connectionString.Value;
                    var keyStoreEncryptionPassword = await keyVaultClient.GetSecretAsync(_options.Value.SecretUri, "KeyStoreEncryptionPassword").ConfigureAwait(false);
                    KeyStoreEncryptionPassword = keyStoreEncryptionPassword.Value;
                }

                _initialized = true;
            }
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(_options.Value.ClientId, _options.Value.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(false);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token.");
            }

            return result.AccessToken;
        }
    }
}
