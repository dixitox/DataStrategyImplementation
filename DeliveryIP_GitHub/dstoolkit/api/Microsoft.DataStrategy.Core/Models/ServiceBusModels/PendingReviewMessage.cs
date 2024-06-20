namespace Microsoft.DataStrategy.Core.Models.ServiceBusModels
{
    public class PendingReviewMessage
    {
        public PendingReviewMessage() { }
        public PendingReviewMessage(string assetId, string review, UserActionMetadata userInfo) {
            AssetId = assetId;
            Review = review;
            UserMetadata = userInfo;
        }
        
        public string AssetId { get; set; }
        public string Review { get; set; }
        public UserActionMetadata UserMetadata { get; set; }

    }
}
