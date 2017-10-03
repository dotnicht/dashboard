using InvestorDashboard.Business.Implementation;
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

            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
