using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class PendingAssetReview
    {
        public PendingAssetReview() { }
        public PendingAssetReview(string assetId, string review)
        {
            Id = Guid.NewGuid().ToString();
            AssetId = assetId;
            Review = review;
        }              
        
        [JsonProperty("partitionKey")]
        virtual public string PartitionKey => "1";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("assetId")]
        public string AssetId { get; set; }


        [JsonProperty("review")]
        public string Review { get; set; }       
        
        [JsonProperty("submittedBy")]
        public UserActionMetadata SubmittedBy { get; set; }
        
    }

}