using Microsoft.DataStrategy.Core.Models.ServiceBusModels;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface ICommunicationService
    {
        public Task SendMailAsync(MailServiceBusMessage message, string template);
    }
}
