using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Web.Server.Models.DashboardModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestorDashboard.Web.Server.RestAPI
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;

        public DashboardController(ApplicationDbContext context, IExchangeRateService exchangeRateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        public async Task<IActionResult> GetIcoStatus()
        {
            var status = new IcoInfoModel
            {
                TotalCoins = _context.Users.Sum(x => x.Balance),
                TotalInvestors = _context.CryptoTransactions.Where(x => x.Direction == CryptoTransactionDirection.Inbound).Select(x => x.CryptoAddress.CryptoAccount.UserId).Distinct().Count(),
                TotalUsd = _context.CryptoTransactions.Where(x => x.Direction == CryptoTransactionDirection.Inbound).Sum(x => x.Amount * x.ExchangeRate),
            };

            return Ok(status);
        }

        public async Task<IActionResult> GetPaymentInfo()
        {
            var userId = string.Empty; // TODO: extract real user id.
            var paymentInfo = _context.CryptoAccounts
                .Include(x => x.CryptoAddresses)
                .Where(x => !x.IsDisabled && x.UserId == userId && x.User.IsEligibleForTokenSale)
                .ToList()
                .Select(async x => new PaymentInfoModel
                {
                    Currency = x.Currency.ToString(),
                    Address = x.CryptoAddresses.FirstOrDefault(y => !y.IsDisabled)?.Address,
                    Rate = await _exchangeRateService.GetExchangeRate(x.Currency)
                })
                .ToArray();

            return Ok(paymentInfo);
        }
    }
}
