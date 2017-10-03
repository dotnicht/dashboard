using System;

namespace InvestorDashboard.Business.Implementation
{
    internal class ConfigurationService : IConfigurationService
    {
        public string KeyVaultClientId => GetValue<string>("KeyVaultClientId");

        public string KeyVaultClientSecret => GetValue<string>("KeyVaultClientSecret");

        public Uri KeyValaultSecretUri => GetValue<Uri>("KeyValaultSecretUri");

        private T GetValue<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // TODO: extract value.
            string setting = string.Empty;

            var type = typeof(T);
            var ci = type.GetConstructor(new[] { typeof(string) });

            if (ci != null)
            {
                return (T)ci.Invoke(new[] { setting });
            }

            return (T)Convert.ChangeType(setting, type);
        }
    }
}