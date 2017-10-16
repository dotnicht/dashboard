using InvestorDashboard.Backend.Models;
using System;

namespace InvestorDashboard.Backend.Services
{
    public interface IExchangeRateService
    {
        decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime? dateTime = null);
        decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime dateTime, bool fallbacktoCurrent);
        void RefreshExchangeRate(Currency baseCurrency);
    }
}
