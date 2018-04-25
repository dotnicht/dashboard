using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IGenericAddressService
    {
        Task CreateMissingAddresses(string userId = null, bool includeInternal = true);
    }
}
