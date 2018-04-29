using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string[] emails, string subject, string message);
    }
}
