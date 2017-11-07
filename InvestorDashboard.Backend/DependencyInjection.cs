using System;
using System.Collections.Generic;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Backend.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;
using static InvestorDashboard.Backend.Services.Implementation.BitcoinService;
using static InvestorDashboard.Backend.Services.Implementation.EthereumService;

namespace InvestorDashboard.Backend
{
    public static class DependencyInjection
    {
        public static void Configure(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // rest services.
            services.AddTransient<IRestService<List<decimal>>, RestService<List<decimal>>>();
            services.AddTransient<IRestService<ChainResponse>, RestService<ChainResponse>>();
            services.AddTransient<IRestService<EtherscanResponse>, RestService<EtherscanResponse>>();

            // crypto currencies services.
            services.AddTransient<IBitcoinService, BitcoinService>();
            services.AddTransient<IEthereumService, EthereumService>();
            services.AddTransient<ICryptoService, BitcoinService>();
            services.AddTransient<ICryptoService, EthereumService>();

            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IExchangeRateService, ExchangeRateService>();
            services.AddTransient<ITokenService, TokenService>();
        }
    }
}
