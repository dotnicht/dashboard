using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class AmazonEmailService : IEmailService
    {
        private readonly IOptions<AmazonEmailSettings> _options;

        public AmazonEmailService(IOptions<AmazonEmailSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using (var client = new AmazonSimpleEmailServiceClient(_options.Value.AccessKeyId, _options.Value.SecretAccessKey, RegionEndpoint.EUWest1))
            {
                var request = new SendEmailRequest
                {
                    Source = _options.Value.Address,
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
                    Destination = new Destination(new List<string> { email })
                };

                await client.SendEmailAsync(request);
            }
        }
    }
}
