using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IReviewsService
    {
        public Task<PagedResult<AssetReview>> GetAssetReviewsAsync(string assetId, int pageSize, int page);
        public Task<PagedResult<PendingAssetReview>> GetPendingReviewsAsync(int pageSize, int page);

        public Task AddNewReviewAsync(PendingAssetReview review, UserActionMetadata submitterMetadata);

        public Task ApprovePendingReviewAsync(string reviewId, UserActionMetadata approverMetadata);

        public Task RejectPendingReviewAsync(string reviewId, UserActionMetadata approverMetadata);

    }
}
