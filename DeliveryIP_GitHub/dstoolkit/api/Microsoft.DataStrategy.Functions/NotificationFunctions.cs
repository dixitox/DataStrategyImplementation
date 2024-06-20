using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.DataStrategy.Functions.AdaptiveCards;
using Microsoft.Extensions.Logging;

namespace Microsoft.DataStrategy.Functions
{
    public class NotificationFunctions
    {
        private Uri _webhookUrl;
        private bool _teamsNotificationEnabled;
        private HttpClient _client;
        private IAdaptiveCardsMapper _mapper;
        ICommunicationService _commService;

        public NotificationFunctions(IAdaptiveCardsMapper mapper, ICommunicationService commService, AppConfig config, IHttpClientFactory httpClientFactory)
        {
            _teamsNotificationEnabled = Uri.TryCreate(config.Teams?.MainIncomingWebHook, UriKind.Absolute, out _webhookUrl) && _webhookUrl.Scheme == Uri.UriSchemeHttps;            
            _commService = commService;
            _client = httpClientFactory.CreateClient();
            _mapper = mapper;            
        }

        [FunctionName("AssetsTeamsNotificationProcessor")]
        public async Task AssetsTeamsNotificationProcessor(ILogger log, [ServiceBusTrigger("%AssetChangeNotificationQueue%", Connection = "ServiceBusConnection")] AssetOperationMessage message)
        {
            log.LogInformation($"C# Executing TeamsNotificationProcessor at: {DateTime.Now}");
            if (!_teamsNotificationEnabled) {
                log.LogInformation($"Teams Webhook is not valid. Skipping notification.");
                return;
            }

            var card = _mapper.MapToAdaptiveCard(message);
            var content = new StringContent(card, Encoding.UTF8, "application/json");
            await _client.PostAsync(_webhookUrl, content);
        }

        [FunctionName("ReviewsTeamsNotificationProcessor")]
        public async Task ReviewsTeamsNotificationProcessor(ILogger log, [ServiceBusTrigger("%AssetReviewNotificationQueue%", Connection = "ServiceBusConnection")] PendingReviewMessage message)
        {
            log.LogInformation($"C# Executing TeamsNotificationProcessor at: {DateTime.Now}");
            if (!_teamsNotificationEnabled)
            {
                log.LogInformation($"Teams Webhook is not valid. Skipping notification.");
                return;
            }
            
            var card = _mapper.MapToAdaptiveCard(message);
            var content = new StringContent(card, Encoding.UTF8, "application/json");
            await _client.PostAsync(_webhookUrl, content);
        }

        
        [FunctionName("EmailProcessor")]
        public async Task EmailProcessor(ILogger log, [ServiceBusTrigger("%MailQueue%", Connection = "ServiceBusConnection")] MailServiceBusMessage message, ExecutionContext context)
        {
            log.LogInformation($"C# Executing EmailProcessor at: {DateTime.Now}");
            var mailTemplateFile = Path.Combine(context.FunctionAppDirectory, "Content", "MailTemplate.html");
            var mailTemplate = await File.ReadAllTextAsync(mailTemplateFile);
            await _commService.SendMailAsync(message, mailTemplate);
        }
    }
}
