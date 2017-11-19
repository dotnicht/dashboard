using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Api.Models.DashboardModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Api.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IMapper _mapper;
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        private ApplicationUser ApplicationUser => _context.Users
                .Include(x => x.CryptoAddresses)
                .SingleOrDefault(x => x.UserName == User.Identity.Name);

        public DashboardController(
            ApplicationDbContext context,
            IExchangeRateService exchangeRateService,
            IDashboardHistoryService dashboardHistoryService,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenSettings> tokenSettings,
            IEnumerable<ICryptoService> cryptoServices,
            IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }


        [HttpGet("ico_status"), ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetIcoStatus()
        {
            var status = GetIcoStatusModel();

            return Ok(status);
        }

        [Authorize, HttpGet("payment_status"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetPaymentInfo()
        {
            if (ApplicationUser != null && !ApplicationUser.IsTokenSaleDisabled && !_tokenSettings.Value.IsTokenSaleDisabled)
            {
                var paymentInfo = GetPaymentInfoModel();

                return Ok(paymentInfo);
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("client_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetClientInfo()
        {
            if (ApplicationUser != null)
            {
                var clientInfo = GetClientInfoModel();

                return Ok(clientInfo);
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("full_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetDashboard()
        {
            if (ApplicationUser != null)
            {
                var dashboard = new Dashboard
                {
                    ClientInfoModel = GetClientInfoModel(),
                    PaymentInfoList = GetPaymentInfoModel(),
                    IcoInfoModel = GetIcoStatusModel()
                };

                return Ok(dashboard);
            }

            return Unauthorized();
        }

        private ClientInfoModel GetClientInfoModel()
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

            return clientInfo;
        }

        private async Task<IcoInfoModel> GetIcoStatusModel()
        {
            return _mapper.Map<IcoInfoModel>(await _dashboardHistoryService.GetLatestHistoryItem());
        }

        private List<PaymentInfoModel> GetPaymentInfoModel()
        {
            var paymentInfo = ApplicationUser.CryptoAddresses
                .Where(x => !x.IsDisabled && x.Type == CryptoAddressType.Investment)
                .ToList()
                .Join(_cryptoServices.Where(x => !x.Settings.Value.IsDisabled), x => x.Currency, x => x.Settings.Value.Currency, (x, y) => new { Address = x, Settings = y.Settings })
                .Select(async x => new PaymentInfoModel
                {
                    Currency = x.Address.Currency.ToString(),
                    Address = x.Address.Address,
                    Rate = await _exchangeRateService.GetExchangeRate(x.Address.Currency),
                    Confirmations = x.Settings.Value.Confirmations
                })
                .Select(m => m.Result)
                .ToList();

            return paymentInfo;
        }
    }
}
