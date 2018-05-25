using InvestorDashboard.Backend.Database.Models;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IGenericAddressService
    {
        Task CreateMissingAddresses(string userId = null, bool includeInternal = true);
        Task<CryptoAddress> EnsureInternalAddress(ApplicationUser user);
        Task UpdateAddress(string userId, Currency currency, CryptoAddressType cryptoAddressType, string address);
    }
}
