using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Api.Models;
using InvestorDashboard.Api.Models.DashboardModels;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> PostTelegramBotData([FromBody]TelegramWebhookViewModel telegramWebhookViewModel)
        {
            if (telegramWebhookViewModel != null
                && telegramWebhookViewModel.Message != null
                && telegramWebhookViewModel.Message.Chat != null
                && telegramWebhookViewModel.Message.From != null)
            {
                _logger.LogInformation($"Incoming webhook message. ID: { telegramWebhookViewModel.Update_id }.");

                await _messageService.HandleIncomingMessage(
                    telegramWebhookViewModel.Message.From.Username,
                    telegramWebhookViewModel.Message.Text,
                    telegramWebhookViewModel.Message.Chat.Id);
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
                return Ok(await GetPaymentInfoModel());
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("client_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public IActionResult GetClientInfo()
        {
            if (ApplicationUser != null)
            {
                return Ok(_mapper.Map<ClientInfoModel>(ApplicationUser));
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("full_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetDashboard()
        {
            if (ApplicationUser != null)
            {
                var dashboard = new DashboardModel
                {
                    ClientInfoModel = _mapper.Map<ClientInfoModel>(ApplicationUser),
                    PaymentInfoList = await GetPaymentInfoModel(),
                    IcoInfoModel = await GetIcoStatusModel()
                };

                return Ok(dashboard);
            }

            return Unauthorized();
        }

        [Authorize, HttpPost("add_question"), Produces("application/json")]
        public IActionResult AddQuestion([FromBody]QuestionModel question)
        {
            return Ok();
        }

        [Authorize, HttpPost("add_token_transfer"), Produces("application/json")]
        public IActionResult AddTokenTransfer([FromBody]TokenTransferModel transfer)
        {
            var response = transfer.ReCaptchaToken;
            string secretKey = "6LdmAjkUAAAAAA0JNsS5nepCqGLgvU7koKwIG4PH";
            var client = new System.Net.WebClient();
            var recaptchaResult = client.DownloadString($"https://www.google.com/recaptcha/api/siteverify?secret={ secretKey }&response={ response }");
            var obj = JObject.Parse(recaptchaResult);
            var status = (bool)obj.SelectToken("success");

            if (status)
            {
                return Ok();
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = "recaptcha validation false"
            });
        }

        private async Task<IcoInfoModel> GetIcoStatusModel()
        {
            var items = await _dashboardHistoryService.GetHistoryItems();
            var result = _mapper.Map<IcoInfoModel>(_tokenSettings.Value);
            result.TotalBtcInvested = items[Currency.BTC].TotalInvested;
            result.TotalEthInvested = items[Currency.ETH].TotalInvested;
            result.TotalInvestors = items.Sum(x => x.Value.TotalInvestors);
            result.TotalUsdInvested = items.Sum(x => x.Value.TotalUsdInvested);
            result.TotalCoinsBought = items.Sum(x => x.Value.TotalCoinsBought);
            result.Currencies = items.ToDictionary(x => x.Key.ToString(), x => x.Value.TotalInvested);
            return result;
        }

        private async Task<List<PaymentInfoModel>> GetPaymentInfoModel()
        {
            var addresses = await ApplicationUser.CryptoAddresses
                .ToAsyncEnumerable()
                .Where(x => !x.IsDisabled && x.Type == CryptoAddressType.Investment)
                .ToArray();

            var paymentInfo = addresses
                .Join(_cryptoServices.Where(x => !x.Settings.Value.IsDisabled), x => x.Currency, x => x.Settings.Value.Currency, (x, y) => new { Address = x, y.Settings })
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
