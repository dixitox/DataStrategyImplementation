using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IPendingReviewsRepository
    {
        public Task<PagedResult<PendingAssetReview>> GetPendingReviewsAsync(int pageSize, int page);  
        public Task<PendingAssetReview> GetPendingReviewAsync(string id);
        public Task DeletePendingReviewAsync(string id);
        public Task UpsertPendingReviewAsync(PendingAssetReview review);
    }
}

