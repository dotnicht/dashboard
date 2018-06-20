using AspNet.Security.OpenIdConnect.Primitives;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Backend.Services.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorDashboard.Backend
{
    public static class Configuration
    {
        private const string _environmentVariableName = "ASPNETCORE_ENVIRONMENT";

        public static EnvironmentValue Environment
        {
            get => Enum.Parse<EnvironmentValue>(System.Environment.GetEnvironmentVariable(_environmentVariableName)); 
        }

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
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.Configure<TelegramSettings>(configuration.GetSection("Telegram"));
            services.Configure<CaptchaSettings>(configuration.GetSection("Captcha"));
            services.Configure<EthereumSettings>(configuration.GetSection("Ethereum"));
            services.Configure<BitcoinSettings>(configuration.GetSection("Bitcoin"));

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IResourceService, ResourceService>();
            services.AddTransient<IRestService, RestService>();
            services.AddTransient<ICalculationService, CalculationService>();
            services.AddTransient<IExternalInvestorService, ExternalInvestorService>();
            services.AddTransient<IReferralService, ReferralService>();
            services.AddTransient<IAffiliateService, AffiliateService>();
            services.AddTransient<IInternalUserService, InternalUserService>();
            services.AddTransient<IKycService, KycService>();
            services.AddTransient<IKeyVaultService, KeyVaultService>();
            services.AddTransient<IMessageService, MessageService>();
            services.AddTransient<ITelegramService, TelegramService>();
            services.AddTransient<IExchangeRateService, ExchangeRateService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IDashboardHistoryService, DashboardHistoryService>();
            services.AddTransient<IGenericAddressService, GenericAddressService>();
            services.AddTransient<ISmartContractService, SmartContractService>();
            services.AddTransient<IBitcoinService, BitcoinService>();
            services.AddTransient<IEthereumService, EthereumService>();
            services.AddTransient<ICryptoService, BitcoinService>();
            services.AddTransient<ICryptoService, EthereumService>();

            services
                .AddIdentity<ApplicationUser, ApplicationRole>(config => config.SignIn.RequireConfirmedEmail = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                options.Lockout.MaxFailedAccessAttempts = 10;

                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
        }

        public enum EnvironmentValue
        {
            Development, Production
        }
    }
}
