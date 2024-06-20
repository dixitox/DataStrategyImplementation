using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IGitHubStatisticsRepository
    {
        Task<GitHubStatistics?> GetStatisticsAsync(string id);
        Task UpsertStatisticsAsync(GitHubStatistics statistics);
    }
}

