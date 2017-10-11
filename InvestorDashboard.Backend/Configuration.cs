using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorDashboard.Backend
{
    public static class Configuration
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<KeyVault>(configuration.GetSection("KeyVault"));
            services.Configure<Bitcoin>(configuration.GetSection("Bitcoin"));
            services.Configure<Ethereum>(configuration.GetSection("Ethereum"));
            services.Configure<ExchangeRate>(configuration.GetSection("ExchangeRate"));
        }
    }
}
