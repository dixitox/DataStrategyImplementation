using Azure.Search.Documents.Indexes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Core.Models
{
    public class Asset : AssetBase
    {
        [SearchableField]
        [Required]
        [JsonProperty("businessProblem")]
        public string BusinessProblem { get; set; }
        [SearchableField]
        [Required]
        [JsonProperty("businessValue")]
        public string BusinessValue { get; set; }
        [SearchableField]
        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }
        [SearchableField]
        [Required]
        [JsonProperty("modelingApproachAndTraining")]
        public string ModelingApproachAndTraining { get; set; }
        [SearchableField]
        [JsonProperty("data")]
        public string Data { get; set; }
        [Required]
        [JsonProperty("architecture")]
        public string Architecture { get; set; }   
        [Required]
        [JsonProperty("screenshot")]
        public Image Screenshot { get; set; }        
        [Required]
        [JsonProperty("repositoryUrl")]
        public string RepositoryUrl { get; set; }
        [JsonProperty("demoUrl")]
        public string DemoUrl { get; set; }
        [JsonProperty("armTemplate")]
        public string ArmTemplate { get; set; }
        [JsonProperty("workflowUrl")]
        public string WorkflowUrl { get; set; }
        [JsonProperty("requireHosting")]        
        public bool RequireHosting { get; set; }
        [JsonProperty("acrRepositoryName")]        
        public string AcrRepositoryName { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
        [JsonProperty("stargazers")]
        public long Stargazers { get; set; }
        [JsonProperty("subscribers")]
        public long Subscribers { get; set; }
        [JsonProperty("forks")]
        public long Forks { get; set; }   
    }    

    public class AssetBase
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";
        [SearchableField(IsSortable = true)]
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }
        [SearchableField]
        [JsonProperty("tagline")]
        [Required]
        public string Tagline { get; set; }        
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [JsonProperty("industries")]
        public List<string> Industries { get; set; }
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [Required]
        [JsonProperty("authors")]
        public List<Author> Authors { get; set; }
        [SimpleField(IsSortable = true)]
        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("importV1")]
        public bool ImportV1 { get; set; }

        [SimpleField(IsSortable = true)]
        [JsonProperty("lastPush")]
        public DateTimeOffset? LastPushed { get; set; }
        [SimpleField(IsSortable = true)]
        [JsonProperty("bookmarked")]
        public long Bookmarked { get; set; }
        [SimpleField(IsSortable = true)]
        [JsonProperty("reviewed")]
        public long Reviewed { get; set; }
        [JsonProperty("createdBy")]
        public UserActionMetadata CreatedBy { get; set; }
        [JsonProperty("releasedBy")]
        public UserActionMetadata ReleasedBy { get; set; }
        [JsonProperty("lastChangedBy")]
        public UserActionMetadata LastChangedBy { get; set; }
        [JsonProperty("assetType")]
        [SimpleField(IsFilterable = true, IsFacetable = true)]
        public List<string> AssetType { get; set; } = new List<string> { };
        [SimpleField(IsFilterable = true)]
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }                
        [JsonProperty("usedBy")]
        public List<IndexedUserMetadata> UsedBy { get; set; } = new List<IndexedUserMetadata> { };
        [SimpleField(IsSortable = true)]
        [JsonProperty("usedByCount")]
        public long UsedByCount { get; set; }

    }

    public class Author
    {
        [SearchableField]
        [JsonProperty("name")]
        public string Name { get; set; }
        [SearchableField]
        [JsonProperty("email")]
        public string Email { get; set; } 
        [SearchableField]
        [JsonProperty("githubAlias")]
        public string GitHubAlias { get; set; }
        [SimpleField]
        [JsonProperty("githubAvatar")]
        public string GitHubAvatar { get; set; }
        [JsonProperty("twitter")]
        public string Twitter { get; set; }
        [JsonProperty("linkedin")]
        public string Linkedin { get; set; }

    } 

}