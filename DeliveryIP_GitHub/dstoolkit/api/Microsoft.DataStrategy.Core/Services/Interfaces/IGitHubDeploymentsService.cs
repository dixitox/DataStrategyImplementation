using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IGitHubDeploymentsService
    {
        public Task<bool> IsSignatureValid(Stream body, string signature);
        public Task<DeploymentsStatus> GetDeploymentStatusAsync(string repoName);
        public Task<bool> DeployEnvironment(Asset asset, DeployEnvironmentEnum environment, UserActionMetadata userMetadata);
        public bool IsEnvironmentDeployable(DeploymentsStatus deploymentStatus, DeployEnvironmentEnum environment);
        public Task ProcessWebhookEventAsync(WorkflowRunGitHubWebhook payload, string signature);

        public Task<List<DeploymentHistory>> GetDeploymentHistories(string repoName, string[] environments);
    }
}
