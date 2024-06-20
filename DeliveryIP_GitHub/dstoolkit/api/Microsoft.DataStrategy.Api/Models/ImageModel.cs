using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class ImageModel
    {           
        [JsonProperty("base64")]
        public string Base64 { get; set; }
    }

}