using InvestorDashboard.Backend.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IDashboardHistoryService : IDisposable
    {
        Task RefreshHistory();
        Task<IDictionary<Currency, DashboardHistoryItem>> GetHistoryItems(DateTime? dateTime = null);
    }
}
