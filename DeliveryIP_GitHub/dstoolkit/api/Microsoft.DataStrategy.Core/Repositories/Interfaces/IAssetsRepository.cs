using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IAssetsRepository
    {
        Task<Asset?> GetAssetAsync(string id);
        Task<PagedResult<AssetBase>> GetAssetsBaseAsync(int pageSize, int page, bool? enabled, string? orderBy, bool descending = true, string filterByUser = "");
        Task<PagedResult<Asset>> GetAssetsAsync(int pageSize, int page, bool onlyEnabled = true);
        Task UpsertAssetAsync(Asset asset);
        Task DeleteAssetAsync(string id);
        Task<int> GetMaxOrder();
        Task<List<Asset>> InitContainerData(string folder, string organizationName);        
    }
}

