using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class GitHubStatisticsRepository : CosmosRepositoryBase, IGitHubStatisticsRepository
    {
        public GitHubStatisticsRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.GitHubStatisticsContainer)
        {
        }
        public async Task UpsertStatisticsAsync(GitHubStatistics statistics) => await UpsertAsync(statistics);
        public async Task<GitHubStatistics?> GetStatisticsAsync(string id) => await GetAsync<GitHubStatistics>("1", id);
    }
}

