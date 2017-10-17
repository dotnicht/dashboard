﻿using InvestorDashboard.Backend.ConfigurationSections;
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

            services.Configure<KeyVaultSettings>(configuration.GetSection("KeyVault"));
            services.Configure<BitcoinSettings>(configuration.GetSection("Bitcoin"));
            services.Configure<EthereumSettings>(configuration.GetSection("Ethereum"));
            services.Configure<ExchangeRateSettings>(configuration.GetSection("ExchangeRate"));
        }
    }
}