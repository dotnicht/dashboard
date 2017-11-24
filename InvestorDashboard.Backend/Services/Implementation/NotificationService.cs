using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class NotificationService : ContextService, INotificationService
    {
        private readonly IEmailService _emailService;

        public NotificationService(ApplicationDbContext context, ILoggerFactory loggerFactory, IEmailService emailService) 
            : base(context, loggerFactory)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task NotifyRegistrationConfirmationRequired(string userId, string message)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var user = Context.Users.Single(x => x.Id == userId);
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", message);
        }
    }
}
