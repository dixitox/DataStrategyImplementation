using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IAssetReviewsRepository
    {        
        public Task<AssetReview> GetAssetReviewsAsync(string id, string assetId);

        public Task<PagedResult<AssetReview>> GetReviewsAsync(string assetId, int pageSize, int page);

        public Task UpsertAssetReviewAsync(AssetReview review);
    }
}

