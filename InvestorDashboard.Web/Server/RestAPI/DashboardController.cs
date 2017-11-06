using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    [Route("api/[controller]"), Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IMapper _mapper;

        private ApplicationUser ApplicationUser => _context.Users
                .Include(x => x.CryptoAddresses)
                .SingleOrDefault(x => x.UserName == User.Identity.Name);

        public DashboardController(ApplicationDbContext context, IExchangeRateService exchangeRateService, UserManager<ApplicationUser> userManager, IOptions<TokenSettings> tokenSettings, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("ico_status")]
        public async Task<IActionResult> GetIcoStatus()
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

            return Ok(status);
        }

        [HttpGet("payment_status")]
        public async Task<IActionResult> GetPaymentInfo()
        {
            if (ApplicationUser != null && !ApplicationUser.IsTokenSaleDisabled && !_tokenSettings.Value.IsTokenSaleDisabled)
            {
                var paymentInfo = ApplicationUser.CryptoAddresses
                    .Where(x => !x.IsDisabled && x.Type == CryptoAddressType.Investment)
                    .ToList()
                    .Select(async x => new PaymentInfoModel
                    {
                        Currency = x.Currency.ToString(),
                        Address = x.Address,
                        Rate = await _exchangeRateService.GetExchangeRate(x.Currency)
                    })
                    .Select(m => m.Result)
                    .ToList();

                return Ok(paymentInfo);
            }

            return Unauthorized();
        }

        [HttpGet("client_info")]
        public async Task<IActionResult> GetClientInfo()
        {
            if (ApplicationUser != null)
            {
                var clientInfo = new ClientInfoModel
                {
                    Balance = ApplicationUser.Balance,
                    BonusBalance = ApplicationUser.BonusBalance,
                    IsTokenSaleDisabled = ApplicationUser.IsTokenSaleDisabled,
                    Address = ApplicationUser.CryptoAddresses
                        .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.ETH && x.Type == CryptoAddressType.Contract)
                        ?.Address
                };

                return Ok(clientInfo);
            }

            return Unauthorized();
        }
    }
}
