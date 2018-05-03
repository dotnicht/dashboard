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
        private readonly ITokenService _tokenService;
        private readonly IReferralService _referralService;
        private readonly IKycService _kycService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IOptions<EthereumSettings> _ethereumSettings;
        private readonly IOptions<ReferralSettings> _referralSettings;
        private readonly IOptions<CommonSettings> _commonSettings;
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardController> _logger;

        private ApplicationUser ApplicationUser => _context.Users
                .Include(x => x.CryptoAddresses)
                .SingleOrDefault(x => x.UserName == User.Identity.Name);

        public DashboardController(
            ApplicationDbContext context,
            IExchangeRateService exchangeRateService,
            IDashboardHistoryService dashboardHistoryService,
            IMessageService messageService,
            ITokenService tokenService,
            IReferralService referralService,
            IKycService kycService,
            UserManager<ApplicationUser> userManager,
            IOptions<TokenSettings> tokenSettings,
            IOptions<EthereumSettings> ethereumSettings,
            IOptions<ReferralSettings> referralSettings,
            IOptions<CommonSettings> commonSettings,
            IEnumerable<ICryptoService> cryptoServices,
            IMapper mapper,
            ILogger<DashboardController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _referralService = referralService ?? throw new ArgumentNullException(nameof(referralService));
            this._kycService = kycService ?? throw new ArgumentNullException(nameof(kycService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _referralSettings = referralSettings ?? throw new ArgumentNullException(nameof(referralSettings));
            _commonSettings = commonSettings ?? throw new ArgumentNullException(nameof(commonSettings));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<IActionResult> GetClientInfo()
        {
            if (ApplicationUser != null)
            {
                return Ok(await GetClientInfoModel());
            }

            return Unauthorized();
        }

        [Authorize, HttpGet("full_info"), ResponseCache(Duration = 30, VaryByHeader = "Authorization", Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetDashboard()
        {
            if (ApplicationUser != null)
            {
                try
                {
                    var dashboard = new DashboardModel
                    {
                        ClientInfoModel = await GetClientInfoModel(),
                        PaymentInfoList = await GetPaymentInfoModel(),
                        IcoInfoModel = await GetIcoStatusModel()
                    };

                    return Ok(dashboard);
                }
                catch (Exception ex)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = ex.Message
                    });
                }
            }

            return Unauthorized();
        }

        [Authorize, HttpPost("add_token_transfer"), Produces("application/json")]
        public async Task<IActionResult> AddTokenTransfer([FromBody]TokenTransferModel transfer)
        {
            if (await _tokenService.IsUserEligibleForTransfer(ApplicationUser.Id))
            {
                var (hash, success) = await _tokenService.Transfer(ApplicationUser.Id, transfer.Address, transfer.Amount);

                if (success)
                {
                    return Ok(hash);
                }
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.ServerError
            });
        }

        [Authorize, HttpGet("referral"), Produces("application/json")]
        public async Task<IActionResult> GetReferralData()
        {
            if (_referralSettings.Value.IsDisabled)
            {
                return BadRequest();
            }

            var result = new List<ReferralInfoModel.ReferralCurrencyItem>();

            foreach (var service in _cryptoServices)
            {
                var (transactions, pending, balance) = await _referralService.GetRererralData(ApplicationUser.Id, service.Settings.Value.Currency);

                var item = new ReferralInfoModel.ReferralCurrencyItem
                {
                    Currency = service.Settings.Value.Currency.ToString(),
                    Pending = pending,
                    Balance = balance,
                    Transactions = transactions
                        .Select(x => new ReferralInfoModel.ReferralCurrencyItem.ReferralCurrencyTransaction { Hash = x.Key, Amount = x.Value })
                        .ToArray(),
                    Address = ApplicationUser.CryptoAddresses
                        .SingleOrDefault(x => x.Currency == service.Settings.Value.Currency && !x.IsDisabled && x.Type == CryptoAddressType.Referral)
                        ?.Address
                };

                result.Add(item);
            }

            var tx = await _kycService.GetKycTransactions(ApplicationUser.Id, _tokenSettings.Value.Bonus.KycBonuses[TokenSettings.BonusSettings.BonusCriterion.Referral].TransationHash);

            var model = new ReferralInfoModel
            {
                Count = tx.Count(),
                Tokens = tx.Sum(x => long.Parse(x.Amount)),
                Items = result.ToArray(),
                Link = string.Format(_referralSettings.Value.UriMask, ApplicationUser.ReferralCode)
            };

            return Ok(model);
        }

        [Authorize, HttpPost("referral")]
        public async Task<IActionResult> PostReferralData([FromBody]ReferralInfoUpdateModel referralInfoUpdateModel)
        {
            if (referralInfoUpdateModel == null)
            {
                throw new ArgumentNullException(nameof(referralInfoUpdateModel));
            }

            if (_referralSettings.Value.IsDisabled)
            {
                return BadRequest();
            }

            var currency = (Currency)Enum.Parse(typeof(Currency), referralInfoUpdateModel.Currency);
            await _referralService.UpdateReferralAddress(ApplicationUser.Id, currency, referralInfoUpdateModel.Address);
            return Ok();
        }

        private async Task<ClientInfoModel> GetClientInfoModel()
        {
            var user = _mapper.Map<ClientInfoModel>(ApplicationUser);

            user.IsEligibleForTransfer = ApplicationUser.IsEligibleForTransfer
                && !_tokenSettings.Value.IsTokenTransferDisabled
                && await _tokenService.IsUserEligibleForTransfer(ApplicationUser.Id);

            user.ThresholdExceeded = user.Balance > _tokenSettings.Value.BalanceThreshold;

            return user;
        }

        private async Task<IcoInfoModel> GetIcoStatusModel()
        {
            var result = _mapper.Map<IcoInfoModel>(_tokenSettings.Value);

            var items = await _dashboardHistoryService.GetHistoryItems();

            result.TotalCoinsBought = items.First().Value.TotalCoinsBoughts;
            result.Currencies = items
                .Select(x => new IcoInfoModel.CurrencyValue { Currency = x.Key.ToString(), Value = x.Value.TotalInvested })
                .ToList();

            result.ContractAddress = _ethereumSettings.Value.ContractAddress;
            result.IsReferralSystemDisabled = _referralSettings.Value.IsDisabled;

            return result;
        }

        private async Task<PaymentInfoModel[]> GetPaymentInfoModel()
        {
            if (ApplicationUser.IsTokenSaleDisabled || (_tokenSettings.Value.IsTokenSaleDisabled && ApplicationUser.Id != _commonSettings.Value.AdminUserId))
            {
                return new PaymentInfoModel[0];
            }

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
                    Rate = await _exchangeRateService.GetExchangeRate(x.Address.Currency, _tokenSettings.Value.Currency),
                    Confirmations = x.Settings.Value.Confirmations
                })
                .Select(m => m.Result)
                .ToArray();

            return paymentInfo;
        }
    }
}
