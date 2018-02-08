using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IExchangeRateService : IDisposable
    {
        Task<decimal> GetExchangeRate(Currency currency, DateTime? dateTime = null);
        Task<decimal> RefreshExchangeRate(Currency currency, DateTime? dateTime = null);
    }
}
