using InvestorDashboard.Backend.Database.Models;

namespace InvestorDashboard.Backend.Services
{
    public interface ICalculationService
    {
        decimal ToDecimalValue(string value, Currency currency);
        string ToStringValue(decimal value, Currency currency);
    }
}
