﻿using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ITokenService
    {
        Task RefreshTokenBalance(string userId = null);
        Task<bool> IsUserEligibleForTransfer(string userId);
        Task<(string Hash, bool Success)> Transfer(string userId, string destinationAddress, BigInteger amount);
        Task RefreshOutboundTransactions();
    }
}
