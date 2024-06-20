using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Core.Models
{
    public class UserBookmarks
    {
        public UserBookmarks(string id)
        {
            Id = id;
            BookmarkedAssets = new List<string>();
        }

        [JsonProperty("id")]
        public string Id;
        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";
        [Required]
        [JsonProperty("bookmarkedAssets")]
        public List<string> BookmarkedAssets { get; set; }
    }
}