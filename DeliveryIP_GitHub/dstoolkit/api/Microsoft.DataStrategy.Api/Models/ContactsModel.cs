using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class ContactsModel
    {
        [Required]
        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("from")]
        [Required]
        public string From { get; set; }
    }

}