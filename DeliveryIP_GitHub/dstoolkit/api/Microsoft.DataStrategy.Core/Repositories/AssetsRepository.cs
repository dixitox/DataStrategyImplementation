using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.Graph.SecurityNamespace;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class AssetsRepository : CosmosRepositoryBase, IAssetsRepository
    {
        public AssetsRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.AssetsContainer)
        {
        }

        public async Task<Asset?> GetAssetAsync(string id) => await GetAsync<Asset>("1", id);
        public async Task DeleteAssetAsync(string id) => await DeleteAsync<Asset>("1", id);

        public async Task<PagedResult<AssetBase>> GetAssetsBaseAsync(int pageSize, int page, bool? enabled, string? orderBy, bool descending = true, string filterByUser = "")
        {
            var query = GetQuery(enabled, orderBy, descending, filterByUser);
            return await RunPaginatedQueryAsync<AssetBase>(query.ToString(), pageSize, page);
        }

        public async Task<PagedResult<Asset>> GetAssetsAsync(int pageSize, int page, bool onlyEnabled = false)
        {
            var query = GetQuery(onlyEnabled ? true : null);
            return await RunPaginatedQueryAsync<Asset>(query.ToString(), pageSize, page);
        }

        private StringBuilder GetQuery(bool? enabled, string? orderBy = null, bool descending = true, string filterByUser = "")
        {
            var query = new StringBuilder();
            query.Append($"SELECT * FROM c WHERE c.partitionKey = '1'");

            if (enabled != null)
            {
                query.Append($" AND c['enabled'] = {enabled.ToString().ToLower()}");
            }
            if (!string.IsNullOrEmpty(filterByUser))
            {
                query.Append($" AND c.createdBy['objectId'] = '{filterByUser}'");
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
            return orderBy == "name" || orderBy == "createdBy['on']" || orderBy == "createdBy['mail']" || orderBy == "enabled";
        }

        public async Task UpsertAssetAsync(Asset asset) => await UpsertAsync(asset);

        public async Task<int> GetMaxOrder()
        {
            var queryDef = new QueryDefinition("SELECT MAX(c[\"order\"]) as Max FROM c");
            var order = await ExeucteQueryAsync<dynamic>(queryDef);
            return order.FirstOrDefault()?.Max ?? 0;
        }

        public async Task<List<Asset>> InitContainerData(string assetsDirectory, string organizationName)
        {
            var assets = new List<Asset>();
            foreach (var file in Directory.GetFiles(assetsDirectory))
            {
                var jsonString = File.ReadAllText(file);
                var asset = JsonConvert.DeserializeObject<Asset>(jsonString);
                asset.RepositoryUrl = string.Format(asset.RepositoryUrl, organizationName);
                await UpsertAssetAsync(asset);
                assets.Add(asset);
            }
            return assets;
        }
    }
}