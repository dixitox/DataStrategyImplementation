using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using System.Net;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class CosmosRepositoryBase
    {
        protected readonly Container Container;

        public CosmosRepositoryBase(CosmosClient dbClient, AppConfig config, string container)
        {
            Container = dbClient.GetContainer(config.Cosmos.Database, container);
        }

        protected async Task UpsertAsync<T>(T document)
        {
            await Container.UpsertItemAsync(document);
        }

        protected async Task<T?> GetAsync<T>(string partitionKey, string id)
        {
            try
            {
                return await Container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default(T);
            }
        }

        protected async Task DeleteAsync<T>(string partitionKey, string id)
        {
            await Container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }

        protected async Task<IReadOnlyCollection<T>> ExeucteQueryAsync<T>(QueryDefinition query)
        {
            var feed = Container.GetItemQueryIterator<T>(query);
            var ret = new List<T>();
            while (feed.HasMoreResults)
            {
                ret.AddRange(await feed.ReadNextAsync());
            }
            return ret;
        }

        protected async Task<PagedResult<T>> RunPaginatedQueryAsync<T>(string queryString, int pageSize, int page, Dictionary<string, object> @params = null)
        {
            queryString += " OFFSET @Offset LIMIT @Limit";
            var query = new QueryDefinition(queryString);
            if (@params != null)
            {
                foreach (var p in @params)
                {
                    query.WithParameter(p.Key, p.Value);
                }
            }
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 1;
            }
            query.WithParameter("@Offset", pageSize * (page - 1));
            query.WithParameter("@Limit", pageSize + 1);

            var rows = await ExeucteQueryAsync<T>(query);
            var hasMorePages = rows.Count > pageSize;
            return new PagedResult<T>
            {
                Values = rows.Take(pageSize).ToList(),
                HasMorePages = hasMorePages,
                Page = page,
                PageSize = pageSize
            };
        }

        protected async Task<List<T>> RunQueryAsync<T>(string queryString, Dictionary<string, object> @params = null)
        {
            var query = new QueryDefinition(queryString);
            if (@params != null)
            {
                foreach (var p in @params)
                {
                    query.WithParameter(p.Key, p.Value);
                }
            }
           
            var rows = await ExeucteQueryAsync<T>(query);
            return rows.ToList();
        }

        protected async Task<List<T>> GetAllAsync<T>()
        {
            return await RunQueryAsync<T>("SELECT * FROM c");
        }
    }
}

