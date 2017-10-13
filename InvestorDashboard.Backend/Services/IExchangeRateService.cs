using InvestorDashboard.Backend.Models;

namespace InvestorDashboard.Backend.Services
{
    public interface IExchangeRateService
    {
        decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency);
    }
}
