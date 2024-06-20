using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class DeploymentHistory
    {
        [JsonProperty("id")]
        public string Id => RepositoryName;

        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";

        [JsonProperty("repositoryName")]
        public string RepositoryName { get; set; }

        [JsonProperty("repositoryUrl")]
        public string RepositoryUrl { get; set; }

        [JsonProperty("environment")]
        public string? Environment { get; set; }

        [JsonProperty("runId")]
        public long RunId { get; set; }

        [JsonProperty("lastUpdatedBy")]
        public UserActionMetadata LastUpdatedBy { get; set; }

        
    }
}
