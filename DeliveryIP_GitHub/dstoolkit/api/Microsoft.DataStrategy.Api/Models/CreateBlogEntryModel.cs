using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class CreateBlogEntryModel
    {   
        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }        
        [Required]
        [JsonProperty("introduction")]
        public string Introduction { get; set; }
        [JsonProperty("heroImage")]
        public Image HeroImage { get; set; }
        [Required]
        [JsonProperty("body")]
        public string Body { get; set; }        
        [JsonProperty("entryType")]
        public BlogEntryType EntryType { get; set; }      
        [JsonProperty("status")]
        public BlogEntryStatus Status { get; set; }        
    }

}