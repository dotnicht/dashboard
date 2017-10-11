using InvestorDashboard.Business.Services;
using InvestorDashboard.Business.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorDashboard.Business
{
    public static class DependencyInjectionConfiguration
    {
        public static void Configure(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<IBitcoinService, BitcoinService>();
            services.AddTransient<IEthereumService, EthereumService>();
            services.AddTransient<IExchangeRateService, ExchangeRateService>();
        }
    }
}
