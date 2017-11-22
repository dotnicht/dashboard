using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ITelegramService
    {
        Task SendDashboardHistoryMessage();
        Task SendMessage(string message);
        Task HandleIncomingMessage(string user, string message);
    }
}
