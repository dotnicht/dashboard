using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface INotificationService : IDisposable
    {
        Task NotifyRegistrationConfirmationRequired(string userId, string message);
    }
}
