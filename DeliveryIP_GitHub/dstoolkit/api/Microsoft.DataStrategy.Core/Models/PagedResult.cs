namespace Microsoft.DataStrategy.Core.Models
{
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalResults { get; set; }
        public bool HasMorePages { get; set; }
        public IEnumerable<T> Values { get; set; }
        public IEnumerable<SearchFacet> Facets { get; set; }        
    }

    public class SearchFacet
    {
        public SearchFacet(string name, Dictionary<string, long> categories, long totalResult)
        {
            Name = name;
            Categories = categories;
            TotalResults = totalResult;
        }

        public string Name { get; set; }
        public long TotalResults { get; set; }
        public Dictionary<string, long> Categories { get; set; }
    }
}