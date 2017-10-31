using System.Text.Encodings.Web;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EmailService : IEmailService
    {
        private readonly IOptions<EmailSettings> _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings ?? throw new System.ArgumentNullException(nameof(emailSettings));
        }

        public async Task SendEmailConfirmationAsync(string email, string link)
        {
            await SendEmailAsync(email, "Confirm your email", $"Please confirm your account by clicking this link: <a href='{ HtmlEncoder.Default.Encode(link) }'>link</a>");
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
