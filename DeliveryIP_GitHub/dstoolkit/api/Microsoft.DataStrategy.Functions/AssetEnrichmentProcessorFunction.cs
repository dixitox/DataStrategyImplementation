using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.DataStrategy.Core.Extensions;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Microsoft.DataStrategy.Functions
{
    public class AssetEnrichmentProcessorFunction
    {
        private IAssetsRepository _assetsRepository;
        private IGitHubService _gitHubService;
        private GitHubClient _client;

        public AssetEnrichmentProcessorFunction(IAssetsRepository assetRepository, IGitHubService gitHubService)
        {
            _assetsRepository = assetRepository;
            _gitHubService = gitHubService;
        }
      

        [FunctionName("AssetEnrichmentProcessor")]
        public async Task AssetEnrichmentProcessor(ILogger log, [ServiceBusTrigger("%AssetsEnrichmentToExecuteQueue%", Connection = "ServiceBusConnection")] PendingAssettMessage message)
        {
            log.LogInformation($"C# Executing Asset enrichment {message.Id} ({message.GitHubUrl}) at: {DateTime.Now}");
                        
            if (!GitHubHelpers.IsValidGitHubRepo(message.GitHubUrl))
                return;
            
            var org = GitHubHelpers.GetOrganizationFromRepoUrl(message.GitHubUrl);
            var repoName = GitHubHelpers.GetRepoNameFromRepoUrl(message.GitHubUrl);

            _client = await _gitHubService.GetClientAsync();
            
            var repo = await _client.Repository.Get(org, repoName);
            var contributors = await _client.Repository.GetAllContributors(org, repoName);

            var asset = await _assetsRepository.GetAssetAsync(message.Id);
            if (asset == null) return;

            asset.Stargazers = repo.StargazersCount;
            asset.Subscribers = repo.SubscribersCount;            
            asset.Forks = repo.ForksCount;
            asset.LastPushed = repo.PushedAt;

            foreach (var author in asset.Authors)
            {
                author.GitHubAvatar = contributors.FirstOrDefault(x => x.Login.Equals(author.GitHubAlias, StringComparison.InvariantCultureIgnoreCase))?.AvatarUrl;
            }

            await _assetsRepository.UpsertAssetAsync(asset);
        }

    }
}
