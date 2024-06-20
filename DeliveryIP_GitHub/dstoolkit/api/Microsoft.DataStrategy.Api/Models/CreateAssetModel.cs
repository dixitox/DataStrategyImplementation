using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class CreateAssetModel
    {
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("tagline")]
        [Required]
        public string Tagline { get; set; }
        [JsonProperty("industries")]
        public List<string> Industries { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [Required]
        [JsonProperty("authors")]
        public List<Author> Authors { get; set; }
        [Required]        
        [JsonProperty("assetType")]
        public List<string> AssetType { get; set; }        
        [Required(ErrorMessage = "The Business problem field is required.")]
        [JsonProperty("businessProblem")]
        public string BusinessProblem { get; set; }
        [Required(ErrorMessage = "The Business value field is required.")]
        [JsonProperty("businessValue")]
        public string BusinessValue { get; set; }
        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }
        [Required(ErrorMessage = "The Modeling approach and training field is required.")]
        [JsonProperty("modelingApproachAndTraining")]
        public string ModelingApproachAndTraining { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [Required]
        [JsonProperty("architecture")]
        public string Architecture { get; set; }
        [JsonProperty("screenshot")]
        public Image Screenshot { get; set; }
        [JsonProperty("repositoryUrl")]
        public string RepositoryUrl { get; set; }
        [JsonProperty("demoUrl")]
        public string DemoUrl { get; set; }
        [JsonProperty("armTemplate")]
        public string ArmTemplate { get; set; }
        [JsonProperty("workflowName")]
        public string WorkflowName { get; set; }
        [Required]        
        [JsonProperty("requireHosting")]
        public bool RequireHosting { get; set; }
        [JsonProperty("acrRepositoryName")]
        public string AcrRepositoryName { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }
    }

}