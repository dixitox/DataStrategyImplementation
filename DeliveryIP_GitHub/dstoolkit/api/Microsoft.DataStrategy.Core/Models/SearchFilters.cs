namespace Microsoft.DataStrategy.Core.Models
{
    public class SearchFilters
    {
        public List<string> Industry { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> AssetType { get; set; } = new List<string>();     
        public List<string> AssetIds { get; set; } = new List<string>();
    }
}