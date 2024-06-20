using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IAssetSearchService
    {
        public Task<PagedResult<AssetBase>> SearchAssetsAsync(int pageSize, int page, string fulltext, SearchFilters filters, SortBy sortBy);
        public Task<IEnumerable<SearchFacet>> GetAllFacetsAsync();
        public Task DeleteAssetAsync(string id);

    }
}
