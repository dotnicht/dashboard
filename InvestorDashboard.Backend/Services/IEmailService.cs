using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
