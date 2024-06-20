using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class AssetReview : PendingAssetReview
    {
        [JsonProperty("partitionKey")]
        override public string PartitionKey => AssetId;

        [JsonProperty("approvedBy")]
        public UserActionMetadata ApprovedBy { get; set; }
    }
  
}