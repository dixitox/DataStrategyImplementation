using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class BlogEntry : BlogEntryBase
    {
        [JsonProperty("body")]
        public string Body { get; set; }        
    }
    
    public class BlogEntryBase
    {
        // Year
        [JsonProperty("partitionKey")]
        public string PartitionKey;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("heroImage")]
        public Image HeroImage { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("createdBy")]
        public UserActionMetadata CreatedBy { get; set; }
        [JsonProperty("lastChangedBy")]
        public UserActionMetadata LastChangedBy { get; set; }

        [JsonProperty("entryType")]
        public BlogEntryType EntryType { get; set; }

        [JsonProperty("status")]
        public BlogEntryStatus Status { get; set; }
    }
  
}