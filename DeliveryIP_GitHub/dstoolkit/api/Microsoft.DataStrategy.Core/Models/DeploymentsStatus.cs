using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class DeploymentsStatus
    {
        [JsonProperty("id")]
        public string Id => RepositoryName;
        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";
        [JsonProperty("repositoryName")]
        public string RepositoryName { get; set; }
        [JsonProperty("repositoryUrl")]
        public string RepositoryUrl { get; set; }
        [JsonProperty("workflowActionUrl")]
        public string WorkflowActionUrl { get; set; }
        [JsonProperty("lastDeployEnvironmentRequest")]
        public DeployEnvironmentEnum LastDeployEnvironmentRequest { get; set; }

        [JsonProperty("environments")]
        public List<Environment> Environments { get; set; }

        public DeploymentsStatus (string repoName, string repoUrl, DeployEnvironmentEnum environment)
        {
            RepositoryName = repoName;
            RepositoryUrl = repoUrl;
            LastDeployEnvironmentRequest = environment;
            Environments = new List<Environment>();  
        }

        public void UpsertEnvironment(DeployEnvironmentEnum environment, UserActionMetadata createdBy)
        {
            LastDeployEnvironmentRequest = environment;
            var env = Environments.FirstOrDefault(e => e.EnvironmentName == environment);
            if (env == null)
                Environments.Add(new Environment(environment, createdBy));
            else
            {
                env.LastStatus = DeployStatusEnum.Requested;
                env.LastUpdatedBy = createdBy;
                env.RunHistory.Add(new WorkflowRunUserAction { LastUpdatedBy = createdBy });
            }            
        }
    }

    public class Environment
    {
        [JsonProperty("environmentName")]
        public DeployEnvironmentEnum EnvironmentName { get; set; }
        [JsonProperty("lastStatus")]
        public DeployStatusEnum LastStatus { get; set; }
        [JsonProperty("lastExecutedRunId")]
        public long LastExecutedRunId { get; set; }
        [JsonProperty("lastUpdatedBy")]
        public UserActionMetadata LastUpdatedBy { get; set; }
        public List<WorkflowRunUserAction> RunHistory { get; set; }

        public Environment(DeployEnvironmentEnum environment, UserActionMetadata userActionMetadata)
        {
            EnvironmentName = environment;
            LastStatus = DeployStatusEnum.Requested;
            LastUpdatedBy = userActionMetadata;
            RunHistory = new List<WorkflowRunUserAction>() { new WorkflowRunUserAction { LastUpdatedBy = userActionMetadata } };
        }
    }

    public class WorkflowRunUserAction {
        [JsonProperty("runId")]
        public long RunId { get; set; }
        [JsonProperty("lastUpdatedBy")]
        public UserActionMetadata LastUpdatedBy { get; set; }
    }       
    
}