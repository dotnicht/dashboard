using System;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Backend.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddTransient<IRestService, RestService>();
            services.AddTransient<IInvestorsService, InvestorsService>();
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
