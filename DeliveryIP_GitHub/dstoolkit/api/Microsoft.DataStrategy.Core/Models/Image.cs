using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{    public class Image
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("altText")]
        public string AlternativeText { get; set; }
        [FieldBuilderIgnore]
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public string Base64 { get; set; }
    }
}
