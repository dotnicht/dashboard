using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Backend.Database;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Api.Models.DashboardModels;
using InvestorDashboard.Backend.Models;

namespace InvestorDashboard.Api.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<TokenSettings> _tokenSettings;

        public PublicController(ApplicationDbContext context,
            IOptions<TokenSettings> tokenSettings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
        }

        [HttpGet("get_ico_status")]
        [ResponseCache(Duration = 30)]
        public IcoInfoModel GetIcoStatus()
        {
            var transactions = _context.CryptoTransactions.Where(x => x.Direction == CryptoTransactionDirection.Inbound && x.CryptoAddress.Type == CryptoAddressType.Investment);

            var status = new IcoInfoModel
            {
                TotalCoins = _tokenSettings.Value.TotalCoins,
                TotalCoinsBought = _context.Users.Sum(x => x.Balance),
                TotalInvestors = transactions
                   .Select(x => x.CryptoAddress.UserId)
                   .Distinct()
                   .Count(),
                TotalUsdInvested = transactions.Sum(x => x.Amount * x.ExchangeRate),
                TokenPrice = _tokenSettings.Value.Price,
                IsTokenSaleDisabled = _tokenSettings.Value.IsTokenSaleDisabled
            };

            return status;
        }
    }
}