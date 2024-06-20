using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class DeploymentsStatusRepository : CosmosRepositoryBase, IDeploymentsStatusRepository
    {
        public DeploymentsStatusRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.DeploymentsStatusContainer)
        {
        }
        public async Task UpsertDeploymentStatusAsync(DeploymentsStatus status) => await UpsertAsync(status);
        public async Task<DeploymentsStatus?> GetDeploymentStatusAsync(string repoName) => await GetAsync<DeploymentsStatus>("1", repoName);
    }
}

