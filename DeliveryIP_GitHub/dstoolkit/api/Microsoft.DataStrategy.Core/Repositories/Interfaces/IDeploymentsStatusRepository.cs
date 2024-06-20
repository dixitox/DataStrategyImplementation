using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IDeploymentsStatusRepository
    {
        Task<DeploymentsStatus?> GetDeploymentStatusAsync(string repoName);
        Task UpsertDeploymentStatusAsync(DeploymentsStatus statistics);

    }
}

