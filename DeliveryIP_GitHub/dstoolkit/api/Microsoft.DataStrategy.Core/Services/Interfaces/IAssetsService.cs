using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IAssetsService
    {
        public Task<Asset?> GetAssetAsync(string id);
        public Task<PagedResult<AssetBase>> GetAssetsBaseAsync(int pageSize, int page, bool? enabled, string? orderBy, bool descending = true, string filterByUser = "");
        public Task CreateAssetAsync(Asset asset, bool enabled = false);
        public Task UpdateAssetAsync(Asset asset);
        public Task DeleteAssetAsync(string id, UserActionMetadata userInfo);

    }
}
