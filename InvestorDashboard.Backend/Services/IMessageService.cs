using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IMessageService : IDisposable
    {
        Task HandleIncomingMessage(string externalUserName, string message, int chatId);
        Task SendRegistrationConfirmationRequiredMessage(string userId, string message);
        Task SendPasswordResetMessage(string userId, string message);
        Task SendDashboardHistoryMessage();
    }
}
