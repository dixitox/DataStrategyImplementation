using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using System.Text;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class PendingReviewsRepository : CosmosRepositoryBase, IPendingReviewsRepository
    {
        public PendingReviewsRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.PendingReviewsContainer)
        {
        }
        public async Task<PagedResult<PendingAssetReview>> GetPendingReviewsAsync(int pageSize, int page)
        {
            var query = GetQuery();
            return await RunPaginatedQueryAsync<PendingAssetReview>(query.ToString(), pageSize, page);
        }
        
        private StringBuilder GetQuery()
        {
            var query = new StringBuilder();
            query.Append($"SELECT * FROM c");          
            return query;
        }

        public async Task<PendingAssetReview> GetPendingReviewAsync(string id) => await GetAsync<PendingAssetReview>("1", id);        
        public async Task DeletePendingReviewAsync(string id) => await DeleteAsync<Asset>("1", id);        
        public async Task UpsertPendingReviewAsync(PendingAssetReview review) => await UpsertAsync(review);
    }
}

