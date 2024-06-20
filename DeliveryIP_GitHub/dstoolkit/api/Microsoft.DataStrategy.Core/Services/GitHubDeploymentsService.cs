using Microsoft.DataStrategy.Core.Extensions;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.DataStrategy.Core.Services
{
    public class GitHubDeploymentsService : IGitHubDeploymentsService
    {
        private readonly ILogger<GitHubDeploymentsService> _logger;
        private readonly IAssetsService _assetService;
        private readonly IGitHubService _gitHubService;
        private readonly IDeploymentsStatusRepository _deploymentRepo;
        private readonly GitHubConfiguration _gitHubConfiguration;
        private const string _gitHubActionUrl = "https://github.com/{0}/{1}/actions/workflows/{2}";

        public GitHubDeploymentsService(ILogger<GitHubDeploymentsService> logger, IGitHubService gitHubService, IDeploymentsStatusRepository deploymentRepo, AppConfig appConfig)
        {
            _logger = logger;
            _gitHubService = gitHubService;
            _deploymentRepo = deploymentRepo;
            _gitHubConfiguration = appConfig.GitHubConfiguration;
        }

        #region webhook
        public async Task<bool> IsSignatureValid(Stream body, string signature)
        {
            byte[] key = Encoding.ASCII.GetBytes(_gitHubConfiguration.Secret);
            string? payload;

            using (var reader = new StreamReader(body, leaveOpen: true))
            {
                body.Position = 0;
                payload = await reader.ReadToEndAsync();
            }

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                {
                    byte[] computedSig = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    var computedSignatureString = "sha256=" + ToHexString(computedSig);
                    return computedSignatureString.Equals(signature);
                }
            }
        }

        private string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }

        public async Task ProcessWebhookEventAsync(WorkflowRunGitHubWebhook payload, string signature)
        {
            if (payload.Installation.Id != _gitHubConfiguration.InstallationId)
                return;

            var repoName = payload.Repository.Name;
            var deploymentStatus = await GetDeploymentStatusAsync(repoName);

            var environmentRequest = deploymentStatus?.Environments.FirstOrDefault(env => env.EnvironmentName == deploymentStatus.LastDeployEnvironmentRequest);

            if (environmentRequest == null)
                return;

            environmentRequest.LastStatus = EnumCasting.ToEnum<DeployStatusEnum>(payload.Workflow_Run.Status);
            environmentRequest.LastExecutedRunId = payload.Workflow_Run.Id;
            environmentRequest.RunHistory.Last().RunId = environmentRequest.LastExecutedRunId;
            if (environmentRequest.LastStatus == DeployStatusEnum.Completed && payload.Workflow_Run.Conclusion != "success")
                environmentRequest.LastStatus = DeployStatusEnum.Failure;

            await _deploymentRepo.UpsertDeploymentStatusAsync(deploymentStatus);
        }

        #endregion
        
        public async Task<bool> DeployEnvironment(Asset asset, DeployEnvironmentEnum environment, UserActionMetadata userMetadata)
        {
            var repoName = GitHubHelpers.GetRepoNameFromRepoUrl(asset.RepositoryUrl);
            var deploymentStatus = await _deploymentRepo.GetDeploymentStatusAsync(repoName);

            if ((deploymentStatus != null && !IsEnvironmentDeployable(deploymentStatus, environment)) || (deploymentStatus == null && environment == DeployEnvironmentEnum.Production))
                return false;

            if (deploymentStatus == null)
                deploymentStatus = new DeploymentsStatus(repoName, asset.RepositoryUrl, environment);

            deploymentStatus.WorkflowActionUrl = asset.WorkflowUrl;

            deploymentStatus.UpsertEnvironment(environment, userMetadata);
            await _deploymentRepo.UpsertDeploymentStatusAsync(deploymentStatus);

            try
            {
                var client = await _gitHubService.GetClientAsync();
                var inputs = new Dictionary<string, object> { { "repositoryUrl", asset.RepositoryUrl }, {"repositoryName", repoName }, { "environment", environment.ToString().ToLower() } };
                var createWorkflowDispatch = new CreateWorkflowDispatch("main") { Inputs = inputs };                
                await client.Actions.Workflows.CreateDispatch(GitHubHelpers.GetOrganizationFromRepoUrl(asset.WorkflowUrl), GitHubHelpers.GetRepoNameFromRepoUrl(asset.WorkflowUrl), GitHubHelpers.GetWorkflowNameFromUrl(asset.WorkflowUrl), createWorkflowDispatch);
            }
            catch (Exception ex)
            {
                deploymentStatus.Environments.FirstOrDefault(env => env.EnvironmentName == deploymentStatus.LastDeployEnvironmentRequest).LastStatus = DeployStatusEnum.Failure;
                await _deploymentRepo.UpsertDeploymentStatusAsync(deploymentStatus);
                throw ex;
            }

            return true;
        }

        public async Task<DeploymentsStatus> GetDeploymentStatusAsync(string repoName)
        {
            var deploymentStatus = await _deploymentRepo.GetDeploymentStatusAsync(repoName);
            return deploymentStatus;
        }

        public bool IsEnvironmentDeployable(DeploymentsStatus deploymentStatus, DeployEnvironmentEnum environment)
        {
            var sandboxStatus = deploymentStatus.Environments.FirstOrDefault(x => x.EnvironmentName == DeployEnvironmentEnum.Sandbox)?.LastStatus;
            var prodStatus = deploymentStatus.Environments.FirstOrDefault(x => x.EnvironmentName == DeployEnvironmentEnum.Production)?.LastStatus;

            if (environment == DeployEnvironmentEnum.Sandbox)
            {
                if (deploymentStatus.LastDeployEnvironmentRequest == DeployEnvironmentEnum.Sandbox)
                {
                    return sandboxStatus == DeployStatusEnum.None || sandboxStatus == DeployStatusEnum.Completed || sandboxStatus == DeployStatusEnum.Failure;
                }

                if (deploymentStatus.LastDeployEnvironmentRequest == DeployEnvironmentEnum.Production)
                {
                    return prodStatus == DeployStatusEnum.None || prodStatus == DeployStatusEnum.Completed || prodStatus == DeployStatusEnum.Failure;
                }
            }

            if (environment == DeployEnvironmentEnum.Production)
            {
                if (deploymentStatus.LastDeployEnvironmentRequest == DeployEnvironmentEnum.Sandbox)
                {
                    return sandboxStatus == DeployStatusEnum.Completed && (prodStatus == null || prodStatus == DeployStatusEnum.None || prodStatus == DeployStatusEnum.Completed || prodStatus == DeployStatusEnum.Failure);
                }

                if (deploymentStatus.LastDeployEnvironmentRequest == DeployEnvironmentEnum.Production)
                {
                    return prodStatus == DeployStatusEnum.None || prodStatus == DeployStatusEnum.Completed || prodStatus == DeployStatusEnum.Failure;
                }
            }

            return false;
        }

        public async Task<List<DeploymentHistory>> GetDeploymentHistories(string repoName, string[] environments)
        {
            var deploymentStatus = await _deploymentRepo.GetDeploymentStatusAsync(repoName);

            var deploymentHistories = new List<DeploymentHistory>();

            foreach (var environment in deploymentStatus.Environments)
            {
                deploymentHistories.AddRange(

                    environment.RunHistory.Select(x => new DeploymentHistory()
                    {

                        RepositoryName = deploymentStatus.RepositoryName,
                        RepositoryUrl = deploymentStatus.RepositoryUrl,
                        Environment = environment.EnvironmentName.ToString(),
                        RunId = x.RunId,
                        LastUpdatedBy = x.LastUpdatedBy

                    }
                    ).ToList());

            }

            return deploymentHistories
                .Where(x => !environments.Any() || environments.Contains(x.Environment))
                .OrderByDescending(x => x.LastUpdatedBy.On).ToList();
        }

    }
}
