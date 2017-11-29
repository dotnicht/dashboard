using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IExchangeRateService : IDisposable
    {
        Task<decimal> GetExchangeRate(Currency baseCurrency, Currency quoteCurrency = Currency.USD, DateTime? dateTime = null);
        Task<decimal> GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime dateTime, bool fallbackToCurrent);
        Task RefreshExchangeRate(Currency baseCurrency);
    }
}
