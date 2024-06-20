using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Models
{
    public class SearchModel
    {
        [JsonProperty("search")]        
        public string Search { get; set; } = "*";
        [JsonProperty("filters")]
        public SearchFilters Filters { get; set; } = new SearchFilters();
        [JsonProperty("sortBy")]
        public SortBy SortBy { get; set; } = SortBy.Relevance;
        [Range(1, 100)]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 40;
        [Range(1, 50)]
        [JsonProperty("page")]
        public int Page { get; set; } = 1;
    }

}