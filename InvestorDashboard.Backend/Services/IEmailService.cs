using System.Threading.Tasks;

namespace InvestorDashboard.Business.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string link);
        Task SendEmailAsync(string email, string subject, string message);
    }
}
