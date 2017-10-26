using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Web.Server.Models.DashboardModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Web.Server.RestAPI
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;

        public DashboardController(ApplicationDbContext context, IExchangeRateService exchangeRateService, UserManager<ApplicationUser> userManager, IOptions<TokenSettings> tokenSettings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
        }

        [HttpGet("ico_status")]
        public async Task<IActionResult> GetIcoStatus()
        {
            var status = new IcoInfoModel
            {
                SellDisabled = _tokenSettings.Value.SellDisabled,
                TotalCoins = _tokenSettings.Value.TotalCoins,
                TotalCoinsBought = _context.Users.Sum(x => x.Balance),
                TotalInvestors = _context.CryptoTransactions.Where(x => x.Direction == CryptoTransactionDirection.Inbound).Select(x => x.CryptoAddress.CryptoAccount.UserId).Distinct().Count(),
                TotalUsdInvested = _context.CryptoTransactions.Where(x => x.Direction == CryptoTransactionDirection.Inbound).Sum(x => x.Amount * x.ExchangeRate),
            };

            status.TotalCoinsBoughtPercent = status.TotalCoinsBought * 100 / status.TotalCoins;

            return Ok(status);
        }

        [HttpGet("payment_status"), Authorize]
        public async Task<IActionResult> GetPaymentInfo()
        {
            var user = _context.Users.SingleOrDefault(x => x.UserName == User.Identity.Name);

            if (user != null)
            {
                var paymentInfo = _context.CryptoAccounts
                    .Include(x => x.CryptoAddresses)
                    .Where(x => !x.IsDisabled && x.UserId == user.Id && x.User.IsEligibleForTokenSale)
                    .ToList()
                    .Select(async x => new PaymentInfoModel
                    {
                        Currency = x.Currency.ToString(),
                        Address = x.CryptoAddresses.FirstOrDefault(y => !x.IsDisabled)?.Address,
                        Rate = await _exchangeRateService.GetExchangeRate(x.Currency)
                    })
                    .Select(m => m.Result)
                    .ToList();

                return Ok(paymentInfo);
            }

            return Ok();
        }

        [HttpGet("client_info"), Authorize]
        public async Task<IActionResult> GetClientInfo()
        {
            var user = _context.Users
                .Include(x => x.CryptoAccounts)
                .ThenInclude(x => x.CryptoAddresses)
                .SingleOrDefault(x => x.UserName == User.Identity.Name);

            if (user != null)
            {
                var clientInfo = new ClientInfoModel
                {
                    Balance = user.Balance,
                    Address = user.CryptoAccounts
                        .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.ETH)
                        ?.CryptoAddresses
                        .SingleOrDefault(x => !x.IsDisabled && x.Type == CryptoAddressType.Contract)
                        ?.Address,
                };

                return Ok(clientInfo);
            }

            return Ok();
        }
    }
}
