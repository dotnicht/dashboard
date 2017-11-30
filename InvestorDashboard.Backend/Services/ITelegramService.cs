using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ITelegramService
    {
        Task SendMessage(string message, int chatId);
    }
}
