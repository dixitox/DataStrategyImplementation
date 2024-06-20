using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using System.Text;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class BlogEntriesRepository : CosmosRepositoryBase, IBlogEntriesRepository
    {
        public BlogEntriesRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.BlogEntriesContainer)
        {
        }

        public async Task<BlogEntry?> GetBlogEntryAsync(string id) => await GetAsync<BlogEntry>("2023", id);
        public async Task DeleteBlogEntryAsync(string id) => await DeleteAsync<BlogEntry>("2023", id);

        public async Task<PagedResult<BlogEntryBase>> GetBlogEntryBaseAsync(int pageSize, int page, bool? onlyPublished, string? orderBy, bool descending = true)
        {
            var query = GetQuery(onlyPublished, orderBy, descending);
            return await RunPaginatedQueryAsync<BlogEntryBase>(query.ToString(), pageSize, page);
        }

        private StringBuilder GetQuery(bool? onlyPublished, string? orderBy = null, bool descending = true)
        {
            var query = new StringBuilder();
            query.Append($"SELECT * FROM c WHERE c.partitionKey = '2023'");

            if (onlyPublished != null && onlyPublished.Value)
            {
                query.Append($" AND c['status'] = '{BlogEntryStatus.Published.ToString().ToLower()}'");
            }            

            if (!string.IsNullOrEmpty(orderBy) && CheckAdmittedOrderBy(orderBy))
            {
                query.Append($" ORDER BY c.{orderBy}");
                if (descending)
                    query.Append(" DESC");
                else
                    query.Append(" ASC");
            }

            return query;
        }

        private bool CheckAdmittedOrderBy(string orderBy)
        {
            return orderBy == "name" || orderBy == "createdBy['on']" || orderBy == "createdBy['mail']" || orderBy == "status";
        }

        public async Task UpsertBlogEntryAsync(BlogEntry entry) => await UpsertAsync(entry);      
    }
}