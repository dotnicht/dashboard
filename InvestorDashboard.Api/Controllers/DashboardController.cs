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
using InvestorDashboard.Api.Models;
using Microsoft.Extensions.Logging;

namespace InvestorDashboard.Api.Controllers
{
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        private ApplicationUser ApplicationUser => _context.Users
                .Include(x => x.CryptoAddresses)
                .SingleOrDefault(x => x.UserName == User.Identity.Name);

        public DashboardController(
            ApplicationDbContext context,
            IExchangeRateService exchangeRateService,
            IDashboardHistoryService dashboardHistoryService,
            IMessageService messageService,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenSettings> tokenSettings,
            IEnumerable<ICryptoService> cryptoServices,
            IMapper mapper, 
            ILogger<DashboardController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }


        [HttpGet("ico_status"), ResponseCache(Duration = 30)]
        public async Task<IActionResult> GetIcoStatus()
        {
            var status = await GetIcoStatusModel();
            return Ok(status);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PostTelegramBotData([FromBody]TelegramBotWebhookViewModel telegramBotWebhookViewModel)
        {
            if (telegramBotWebhookViewModel != null && telegramBotWebhookViewModel.Message != null)
            {
                _logger.LogInformation($"Incoming webhook message. ID: { telegramBotWebhookViewModel.Update_id }.");
                await _messageService.HandleIncomingMessage(telegramBotWebhookViewModel.Message.From?.Username, telegramBotWebhookViewModel.Message.Text);
            }
            else 
            {
                _logger.LogError($"Invalid incoming webhook message.");
            }

            return Ok();
        }

        [Authorize, HttpGet("payment_status"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetPaymentInfo()
        {
            if (ApplicationUser != null && !ApplicationUser.IsTokenSaleDisabled && !_tokenSettings.Value.IsTokenSaleDisabled)
            {
                var paymentInfo = await GetPaymentInfoModel();
                return Ok(paymentInfo);
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("client_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetClientInfo()
        {
            if (ApplicationUser != null)
            {
                var clientInfo = await GetClientInfoModel();

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
                    ClientInfoModel = await GetClientInfoModel(),
                    PaymentInfoList = await GetPaymentInfoModel(),
                    IcoInfoModel = await GetIcoStatusModel()
                };
                return Ok(dashboard);
            }

            return Unauthorized();
        }

        private async Task<ClientInfoModel> GetClientInfoModel()
        {
            var address = await ApplicationUser.CryptoAddresses
                .ToAsyncEnumerable()
                .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.ETH && x.Type == CryptoAddressType.Contract);

            var clientInfo = new ClientInfoModel
            {
                Balance = ApplicationUser.Balance,
                BonusBalance = ApplicationUser.BonusBalance,
                IsTokenSaleDisabled = ApplicationUser.IsTokenSaleDisabled,
                Address = address?.Address
            };

            return clientInfo;
        }

        private async Task<IcoInfoModel> GetIcoStatusModel()
        {
            return _mapper.Map<IcoInfoModel>(await _dashboardHistoryService.GetLatestHistoryItem());
        }

        private async Task<List<PaymentInfoModel>> GetPaymentInfoModel()
        {
            var addresses = await ApplicationUser.CryptoAddresses
                .ToAsyncEnumerable()
                .Where(x => !x.IsDisabled && x.Type == CryptoAddressType.Investment)
                .ToArray();

            var paymentInfo = addresses
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
