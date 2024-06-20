using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.DataStrategy.Core.Extensions;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace Microsoft.DataStrategy.Core.Services
{
    public class AssetSearchService : IAssetSearchService
    {
        private SearchClient _searchClient { get; set; }
        private IReadOnlyCollection<string> _selectFields = new List<string>() { "id", "name", "tagline", "industries", "tags", "order", "bookmarked", "lastPush", "authors/name", "authors/email", "authors/githubAlias", "authors/githubAvatar", "releasedBy/on", "lastChangedBy/on", "assetType", "usedBy/displayName", "usedBy/mail", "usedBy/objectId", "usedByCount" };
        private IReadOnlyCollection<string> _facetsFields = new List<string>() { "industries,count:20,sort:value", "tags,count:20", "assetType" };

        public AssetSearchService(AppConfig config)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var options = new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer(serializerSettings) };
            _searchClient = new SearchClient(new Uri(config.Search.Endpoint), config.Search.AssetsIndex, new DefaultAzureCredential(), options);
        }


        public async Task<PagedResult<AssetBase>> SearchAssetsAsync(int pageSize, int page, string fulltext, SearchFilters filters, SortBy sortBy)
        {
            var options = new SearchOptions();
            options.IncludeTotalCount = true;
            options.Facets.AddRange(_facetsFields);
            options.Select.AddRange(_selectFields);
            options.Size = pageSize;
            options.Skip = (page - 1) * pageSize;
            options.Filter = GetFilters(filters);
            SetSortingField(sortBy, fulltext, options);
            SearchResults<AssetBase> results = await _searchClient.SearchAsync<AssetBase>(fulltext, options);

            return new PagedResult<AssetBase>
            {
                Values = results.GetResults().Select(x => x.Document),
                TotalResults = (long)results.TotalCount,
                Page = page,
                PageSize = pageSize,
                Facets = await GetFacetsAsync(fulltext, filters, results)
            };
        }

        private async Task<IEnumerable<SearchFacet>> GetFacetsAsync(string fulltext, SearchFilters filters, SearchResults<AssetBase> results)
        {
            if (!filters.Tags.Any() && !filters.Industry.Any() && !filters.AssetType.Any())
                return results.Facets.Select(x => new SearchFacet(x.Key, x.Value.ToDictionary(y => y.Value.ToString(), y => (long)y.Count), results.TotalCount.Value)).ToList();

            var industryFacets = await SearchFacetsAsync(fulltext, "industries", filters);
            var tagsFacets = await SearchFacetsAsync(fulltext, "tags", filters);
            var assetTypeFacets = await SearchFacetsAsync(fulltext, "assetType", filters);
            return tagsFacets.Union(industryFacets).Union(assetTypeFacets);
        }

        private async Task<IEnumerable<SearchFacet>> SearchFacetsAsync(string fulltext, string facet = null, SearchFilters filters = null)
        {
            var options = new SearchOptions();
            options.IncludeTotalCount = true;
            options.Facets.Add($"{facet},count:20");
            options.Select.Add("id");
            options.Size = 1;
            options.Filter = GetFilters(filters, facet);
            SearchResults<AssetBase> results = await _searchClient.SearchAsync<AssetBase>(fulltext, options);

            return results.Facets.Where(f => f.Key == facet)
                    .Select(x => new SearchFacet(x.Key, x.Value.ToDictionary(y => y.Value.ToString(), y => (long)y.Count), results.TotalCount.Value)).ToList();
        }

        public async Task<IEnumerable<SearchFacet>> GetAllFacetsAsync()
        {
            var options = new SearchOptions();
            options.IncludeTotalCount = true;
            options.Facets.AddRange(_facetsFields);            
            options.Select.Add("id");
            options.Size = 1;
            options.Filter = GetFilters(new SearchFilters());
            SearchResults<AssetBase> results = await _searchClient.SearchAsync<AssetBase>("*", options);
            return results.Facets.Select(x => new SearchFacet(x.Key, x.Value.ToDictionary(y => y.Value.ToString(), y => (long)y.Count), results.TotalCount.Value)).ToList();
        }

        private string GetFilters(SearchFilters filters, string facet = null)
        {
            var filterQuery = new StringBuilder();
            filterQuery.Append("enabled eq true");

            if (filters != null)
            {
                if (filters.Industry != null && filters.Industry.Any() && facet != "industries")
                    filterQuery.Append($" and industries/any(g: search.in(g, '{string.Join(",", filters.Industry.ToArray())}', ',')) ");

                if (filters.Tags != null && filters.Tags.Any() && facet != "tags")
                    filterQuery.Append($" and tags/any(g: search.in(g, '{string.Join(",", filters.Tags.ToArray())}', ',')) ");

                if (filters.AssetType != null && filters.AssetType.Any() && facet != "assetType")
                    filterQuery.Append($" and assetType/any(g: search.in(g, '{string.Join(",", filters.AssetType.ToArray())}', ',')) ");
                
                if (filters.AssetIds.Any())
                    filterQuery.Append($" and search.in(id, '{string.Join(",", filters.AssetIds.ToArray())}', ',') ");
            }

            return filterQuery.ToString();
        }

        private void SetSortingField(SortBy sortBy, string fullText, SearchOptions options)
        {

            if (sortBy == SortBy.Relevance && (string.IsNullOrEmpty(fullText) || fullText == "*") || sortBy == SortBy.Order)
                options.OrderBy.Add("order");

            if (sortBy == SortBy.Name)
                options.OrderBy.Add("name asc");

            if (sortBy == SortBy.RecentlyUpdated)
                options.OrderBy.Add("lastPush desc");

            if (sortBy == SortBy.MostUsedBy)
                options.OrderBy.Add("usedByCount desc");

            if (sortBy == SortBy.MostReviewed)
                options.OrderBy.Add("reviewed desc");           
        }

        public async Task DeleteAssetAsync(string id)
        {
            await _searchClient.DeleteDocumentsAsync("id", new List<string> { id });
        }
    }
}
