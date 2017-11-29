using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class SendGridEmailService : IEmailService
    {
        private readonly IOptions<SendGridEmailSettings> _emailSettings;

        public SendGridEmailService(IOptions<SendGridEmailSettings> emailSettings)
        {
            _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
        }
  
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = new SendGridMessage
            {
                Subject = subject,
                HtmlContent = message,
                From = new EmailAddress(_emailSettings.Value.Address)
            };

            msg.AddTo(email);

            var client = new SendGridClient(_emailSettings.Value.ApiKey);
            await client.SendEmailAsync(msg);
        }
    }
}
