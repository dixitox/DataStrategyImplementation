using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using System.Text;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class AssetReviewsRepository : CosmosRepositoryBase, IAssetReviewsRepository
    {
        public AssetReviewsRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.ReviewsContainer)
        {
        }      

        public async Task<AssetReview> GetAssetReviewsAsync(string id, string assetId) => await GetAsync<AssetReview>(assetId, id);

        public async Task<PagedResult<AssetReview>> GetReviewsAsync(string assetId, int pageSize, int page)
        {
            var query = GetQuery(assetId);
            return await RunPaginatedQueryAsync<AssetReview>(query.ToString(), pageSize, page);
        }

        private StringBuilder GetQuery(string assetId)
        {
            var query = new StringBuilder();
            query.Append($"SELECT * FROM c WHERE c.partitionKey = '{assetId}' ORDER BY c['submittedBy.on'] DESC");
            return query;
        }

        public async Task UpsertAssetReviewAsync(AssetReview review) => await UpsertAsync(review);
    }
}

