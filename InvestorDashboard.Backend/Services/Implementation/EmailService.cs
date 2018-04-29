using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly Dictionary<EmailSettings.EmailProvider, Func<IOptions<EmailSettings>, string[], string, string, Task>> _mapping
            = new Dictionary<EmailSettings.EmailProvider, Func<IOptions<EmailSettings>, string[], string, string, Task>>
        {
            { EmailSettings.EmailProvider.Amazon, SendEmailAmazon },
            { EmailSettings.EmailProvider.SendGrid, SendEmailSendGrid },
        };

        private readonly IOptions<EmailSettings> _options;

        public EmailService(IOptions<EmailSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SendEmailAsync(string[] emails, string subject, string message)
        {
            if (emails == null)
            {
                throw new ArgumentNullException(nameof(emails));
            }

            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await _mapping[_options.Value.Provider](_options, emails, subject, message);
        }

        private static async Task SendEmailAmazon(IOptions<EmailSettings> options, string[] emails, string subject, string message)
        {
            using (var client = new AmazonSimpleEmailServiceClient(options.Value.Amazon.AccessKeyId, options.Value.Amazon.SecretAccessKey, RegionEndpoint.EUWest1))
            {
                var request = new SendEmailRequest
                {
                    Source = options.Value.FromAddress,
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = Encoding.UTF8.BodyName,
                                Data = message
                            }
                        }
                    },
                    Destination = new Destination(emails.ToList())
                };

                await client.SendEmailAsync(request);
            }
        }

        private static async Task SendEmailSendGrid(IOptions<EmailSettings> options, string[] emails, string subject, string message)
        {
            var msg = new SendGrid.Helpers.Mail.SendGridMessage
            {
                Subject = subject,
                HtmlContent = message,
                From = new SendGrid.Helpers.Mail.EmailAddress(options.Value.FromAddress)
            };

            msg.AddTos(emails.Select(x => new SendGrid.Helpers.Mail.EmailAddress(x)).ToList());

            var client = new SendGridClient(options.Value.SendGrid.ApiKey);
            await client.SendEmailAsync(msg);
        }
    }
}
