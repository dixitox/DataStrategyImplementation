using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Core.Models
{
    public class Tag
    {       
        public Tag(string name)
        {
            Name = name;
        }

        [JsonProperty("id")]
        public string Id => Name.Trim();
        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";
        [Required]
        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}