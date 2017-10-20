using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class KeyVaultService : IKeyVaultService
    {
        private readonly IOptions<KeyVaultSettings> _options;

        private string _databaseConnectionString;
        private string _keyStoreEncryptionPassword;

        public string DatabaseConnectionString => _databaseConnectionString ?? (_databaseConnectionString = GetSecret("DatabaseConnectionString").Result);
        public string KeyStoreEncryptionPassword => _keyStoreEncryptionPassword ?? (_keyStoreEncryptionPassword = GetSecret("KeyStoreEncryptionPassword").Result);

        public KeyVaultService(IOptions<KeyVaultSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private async Task<string> GetSecret(string secretName)
        {
            using (var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken)))
            {
                var secret = await keyVaultClient.GetSecretAsync(_options.Value.SecretUri, secretName);
                return secret.Value;
            }
        }

        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(_options.Value.ClientId, _options.Value.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCredential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token.");
            }

            return result.AccessToken;
        }
    }
}
