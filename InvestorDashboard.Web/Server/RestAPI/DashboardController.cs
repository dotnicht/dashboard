using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Web.Server.Models.DashboardModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestorDashboard.Web.Server.RestAPI
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, IExchangeRateService exchangeRateService, UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet("ico_status")]
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

        [HttpGet("payment_status")]
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
                    .ToArray();

                return Ok(paymentInfo);
            }

            return Ok();
        }
    }
}
