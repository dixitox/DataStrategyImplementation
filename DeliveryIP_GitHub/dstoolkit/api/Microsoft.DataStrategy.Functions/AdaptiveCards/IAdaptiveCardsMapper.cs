using Microsoft.DataStrategy.Core.Models.ServiceBusModels;

namespace Microsoft.DataStrategy.Functions.AdaptiveCards
{
    public interface IAdaptiveCardsMapper
    {
        public string MapToAdaptiveCard(AssetOperationMessage message);
        public string MapToAdaptiveCard(PendingReviewMessage message);
    }
}
