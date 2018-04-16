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

            services.Configure<CommonSettings>(configuration.GetSection("Common"));
            services.Configure<KeyVaultSettings>(configuration.GetSection("KeyVault"));
            services.Configure<ReferralSettings>(configuration.GetSection("Referral"));
            services.Configure<JobsSettings>(configuration.GetSection("Jobs"));
            services.Configure<TokenSettings>(configuration.GetSection("Token"));
            services.Configure<ExchangeRateSettings>(configuration.GetSection("ExchangeRate"));
            services.Configure<SendGridEmailSettings>(configuration.GetSection("SendGridEmail"));
            services.Configure<AmazonEmailSettings>(configuration.GetSection("AmazonEmail"));
            services.Configure<TelegramSettings>(configuration.GetSection("Telegram"));
            services.Configure<CaptchaSettings>(configuration.GetSection("Captcha"));
            services.Configure<EthereumSettings>(configuration.GetSection("Ethereum"));
            services.Configure<BitcoinSettings>(configuration.GetSection("Bitcoin"));
        }
    }
}
