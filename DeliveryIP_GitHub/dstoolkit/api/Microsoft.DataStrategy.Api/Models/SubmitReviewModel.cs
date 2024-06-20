using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class SubmitReviewModel
    {
        [Required]
        [JsonProperty("review")]
        public string Review { get; set; }    
        [Required]
        [JsonProperty("assetId")]
        public string AssetId { get; set; }
        [Required]
        [JsonProperty("usedInCustomer")]
        public bool UsedInCustomer { get; set; }
    }

}