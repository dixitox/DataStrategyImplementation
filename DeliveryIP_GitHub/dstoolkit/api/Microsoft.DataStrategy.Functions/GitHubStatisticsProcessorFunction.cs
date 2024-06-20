using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Extensions;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Microsoft.DataStrategy.Functions
{
    public class GitHubStatisticsProcessorFunction
    {
        private IGitHubStatisticsRepository _gitHubStatisticsRepository;
        private IGitHubService _gitHubService;
        private GitHubClient _client;
        private string _organization;
        private bool _gatherTrafficStatistics;


        public GitHubStatisticsProcessorFunction(IGitHubStatisticsRepository gitHubStatisticsRepository, IGitHubService gitHubService, AppConfig config)
        {
            _gitHubService = gitHubService;
            _gitHubStatisticsRepository = gitHubStatisticsRepository;
            _organization = config.GitHubConfiguration.Organization;
            _gatherTrafficStatistics = config.GitHubConfiguration.UsePAT;
        }             
       

        [FunctionName("GitHubStatisticProcessor")]
        public async Task ProcessAssetStatistics(ILogger log, [ServiceBusTrigger("%AssetsStatisticsToExecuteQueue%", Connection = "ServiceBusConnection")] PendingAssettMessage message)
        {
            log.LogInformation($"C# Executing Asset statistics {message.Id} ({message.GitHubUrl}) at: {DateTime.Now}");

            var currentStatistics = await _gitHubStatisticsRepository.GetStatisticsAsync(message.Id);
            if (currentStatistics == null)
                currentStatistics = new GitHubStatistics(message.Id, message.GitHubUrl);
            currentStatistics.LastJobExecution = DateTime.Now;
                        
            if (!GitHubHelpers.IsGitHubRepoRoot(message.GitHubUrl))
            {
                currentStatistics.Valid = false;
                currentStatistics.Comments = "Asset url is not valid: expected format https://github.com/<organization>/<repository>";
                await _gitHubStatisticsRepository.UpsertStatisticsAsync(currentStatistics);
                return;
            }

            var org = GitHubHelpers.GetOrganizationFromRepoUrl(message.GitHubUrl);
            var repo = GitHubHelpers.GetRepoNameFromRepoUrl(message.GitHubUrl);

            _client = await _gitHubService.GetClientAsync();

            if (org == _organization && _gatherTrafficStatistics)
            {
                await GetTrafficViewsAsync(org, repo, currentStatistics);
                await GetTrafficClonesAsync(org, repo, currentStatistics);
                await GetTrafficReferrerAsync(org, repo, currentStatistics);
            }

            await GetContributorsAsync(org, repo, currentStatistics);
            await GetRepositoryInfoAsync(org, repo, currentStatistics);
            await _gitHubStatisticsRepository.UpsertStatisticsAsync(currentStatistics);
        }

        #region statistics

        private async Task GetRepositoryInfoAsync(string organization, string repo, GitHubStatistics currentStatistics)
        {
            var repoInfo = await _client.Repository.Get(organization, repo);
            currentStatistics.LastPushed = repoInfo.PushedAt;
            currentStatistics.Stargazers = repoInfo.StargazersCount;
            currentStatistics.Subscribers = repoInfo.SubscribersCount;
            currentStatistics.Forks = repoInfo.ForksCount;
            currentStatistics.Valid = true;
        }

        private async Task GetContributorsAsync(string organization, string repo, GitHubStatistics currentStatistics)
        {
            var contributors = await _client.Repository.GetAllContributors(organization, repo);
            currentStatistics.Contributors = contributors.Select(x => new RepoContributor { Alias = x.Login, Avatar = x.AvatarUrl, Contributions = x.Contributions }).ToList();
        }

        private async Task GetTrafficReferrerAsync(string organization, string repo, GitHubStatistics currentStatistics)
        {
            if (currentStatistics.LastReferralAggregation == null || (DateTime.Now - currentStatistics.LastReferralAggregation).Value.TotalDays >= 14)
            {
                var referrals = await _client.Repository.Traffic.GetAllReferrers(organization, repo);
                foreach (var referral in referrals)
                {
                    var existing = currentStatistics.Referrers.FirstOrDefault(x => x.Url.Equals(referral.Referrer, StringComparison.InvariantCultureIgnoreCase));
                    if (existing != null)
                    {
                        existing.Uniques += referral.Uniques;
                        existing.Count += referral.Count;
                    }
                    else
                        currentStatistics.Referrers.Add(new ReferrerStatistics(referral));
                }
                currentStatistics.LastReferralAggregation = DateTime.Now;
            }
        }

        private async Task GetTrafficViewsAsync(string organization, string repo, GitHubStatistics currentStatistics)
        {
            var views = await _client.Repository.Traffic.GetViews(organization, repo, new RepositoryTrafficRequest(TrafficDayOrWeek.Week));

            foreach (var view in views.Views)
            {
                currentStatistics.Views.RemoveAll(x => x.Week == view.Timestamp);
                currentStatistics.Views.Add(new WeeklyStatistic(view));
            }
        }

        private async Task GetTrafficClonesAsync(string organization, string repo, GitHubStatistics currentStatistics)
        {
            var clones = await _client.Repository.Traffic.GetClones(organization, repo, new RepositoryTrafficRequest(TrafficDayOrWeek.Week));

            foreach (var clone in clones.Clones)
            {
                currentStatistics.Clones.RemoveAll(x => x.Week == clone.Timestamp);
                currentStatistics.Clones.Add(new WeeklyStatistic(clone));
            }
        }

        #endregion

       

    }
}
