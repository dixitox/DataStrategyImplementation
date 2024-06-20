using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{   
    public class UserActionMetadata
    {
        [SimpleField(IsSortable = true)]
        [JsonProperty("on")]
        public DateTime? On { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("objectId")]
        public string ObjectId { get; set; }
        [JsonProperty("mail")]
        public string Mail { get; set; }
    }    

    public class IndexedUserMetadata
    {
        [SimpleField()]
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [SimpleField()]
        [JsonProperty("objectId")]
        public string ObjectId { get; set; }
        [SimpleField()]
        [JsonProperty("mail")]
        public string Mail { get; set; }
    }
}
