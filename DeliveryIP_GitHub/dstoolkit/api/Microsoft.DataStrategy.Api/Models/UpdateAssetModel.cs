using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class UpdateAssetModel : CreateAssetModel
    {        
        [Required]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }

}