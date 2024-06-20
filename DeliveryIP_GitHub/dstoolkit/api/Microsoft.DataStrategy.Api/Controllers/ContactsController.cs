using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Api.Models;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Text.Json;

namespace Microsoft.DataStrategy.Api.Controllers
{
    [Authorize(Policy = PlatformPolicies.Consumer)]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ContactsController : CustomControllerBase
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly ILogger<ContactsController> _logger;
        private readonly List<string> _mailRecipients;

        public ContactsController(ILogger<ContactsController> logger, AppConfig config)
        {
            _logger = logger;
            var serviceBusClient = new ServiceBusClient(config.ServiceBusConnection.FullyQualifiedNamespace, new DefaultAzureCredential());
            _serviceBusSender = serviceBusClient.CreateSender(config.ServiceBusConnection.MailQueue);
            _mailRecipients = config.CommunicationServiceConfiguration.PlatformContactRecipients.Split(';').ToList();
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:ScopesName")]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Post(ContactsModel message)
        {     
            // todo from address validation, rate limit requests to avoid mail flooding                       

            var internalMessage = new MailServiceBusMessage
            {
                Bcc = _mailRecipients,
                Subject = "Data Science Toolkit - Contact Request",
                Body = $"New Contact request from { User.GetDisplayName() } <br /><br />{message.Body}",
                To = new List<string> { User.GetDisplayName() }
            };

            await SendMailMessageAsync(internalMessage);            
            return Ok();
        }

        private async Task SendMailMessageAsync(MailServiceBusMessage message)
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }

    }
}