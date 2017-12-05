﻿using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService : IDisposable
    {
        IOptions<CryptoSettings> Settings { get; }
        Task CreateCryptoAddress(string userId);
        Task RefreshInboundTransactions();
        Task TransferAssets();
        Task<string> PublishTransaction(CryptoAddress sourceAddress, string destinationAddress, decimal? amount = null);
    }
}
