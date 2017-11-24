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

            services.AddTransient<ICsvService, CsvService>();
            services.AddTransient<IRestService, RestService>();
            services.AddTransient<IInvestorService, InvestorService>();
            services.AddTransient<IAffiliateService, AffiliateService>();
            services.AddTransient<IInternalUserService, InternalUserService>();
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<IMessageService, MessageService>();
            services.AddTransient<ITelegramService, TelegramService>();
            services.AddTransient<IEmailService, SendGridEmailService>();
            services.AddTransient<IExchangeRateService, ExchangeRateService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IDashboardHistoryService, DashboardHistoryService>();

            services.AddTransient<IBitcoinService, BitcoinService>();
            services.AddTransient<IEthereumService, EthereumService>();
            services.AddTransient<ICryptoService, BitcoinService>();
            services.AddTransient<ICryptoService, EthereumService>();
        }
    }
}
