using Azure.Communication.Email;
using Azure.Communication.Email.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.DataStrategy.Core.Services
{
    public class CommunicationService : ICommunicationService
    {        
        private ILogger<CommunicationService> _logger;
        private readonly string _connectionString;
        private readonly string _mailSender;
        

        public CommunicationService(ILogger<CommunicationService> logger, AppConfig config)
        {
            _connectionString = config.CommunicationServiceConfiguration.ConnectionString;
            _mailSender = config.CommunicationServiceConfiguration.EmailSender;
            _logger = logger;
        }
      
        public async Task SendMailAsync(MailServiceBusMessage message, string template)
        {
            var emailClient = new EmailClient(_connectionString);
            var to = message.To?.Select(x => new EmailAddress(x)) ?? new List<EmailAddress>();
            var cc = message.Cc?.Select(x => new EmailAddress(x)) ?? new List<EmailAddress>();
            var bcc = message.Bcc?.Select(x => new EmailAddress(x)) ?? new List<EmailAddress>();            
            var recipients = new EmailRecipients(to, cc, bcc);            
            var body = new EmailContent(message.Subject);            
            body.PlainText = message.Body;
            body.Html = string.Format(template, message.Body);
            await emailClient.SendAsync(new EmailMessage(_mailSender, body, recipients));
        }
        
    }
}
