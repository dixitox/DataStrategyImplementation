using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Octokit;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IGitHubService
    {
        public Task<GitHubClient> GetClientAsync();
    }
}
