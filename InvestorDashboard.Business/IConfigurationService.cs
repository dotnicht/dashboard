using System;

namespace InvestorDashboard.Business
{
    public interface IConfigurationService
    {
        string KeyVaultClientId { get; }
        string KeyVaultClientSecret { get; }
        Uri KeyValaultSecretUri { get; }
    }
}