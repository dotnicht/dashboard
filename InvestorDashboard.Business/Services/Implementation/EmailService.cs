using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace InvestorDashboard.Business.Services.Implementation
{
    internal class EmailService : IEmailService
    {
        public Task SendEmailConfirmationAsync(string email, string link)
        {
            return SendEmailAsync(email, "Confirm your email", $"Please confirm your account by clicking this link: <a href='{ HtmlEncoder.Default.Encode(link) }'>link</a>");
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }
}
