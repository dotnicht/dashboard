using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IDashboardHistoryService : IDisposable
    {
        Task RefreshHistory();
        Task<DashboardHistoryItem> GetLatestHistoryItem(bool includeCurrencies = false);
        Task<DashboardHistoryItem> GetClosestHistoryItem(DateTime dateTime);
    }
}
