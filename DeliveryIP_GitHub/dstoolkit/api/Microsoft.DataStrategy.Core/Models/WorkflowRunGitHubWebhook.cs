using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Models
{
    public class WorkflowRunGitHubWebhook
    {
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("repository")]
        public RepositoryEventModel Repository { get; set; }
        [JsonProperty("workflow_run")]
        public WorkflowRunEventModel Workflow_Run { get; set; }
        [JsonProperty("installation")]
        public InstallationEventModel Installation { get; set; }

        public class RepositoryEventModel
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class WorkflowRunEventModel
        {
            [JsonProperty("id")]
            public long Id { get; set; }
            [JsonProperty("status")]
            public string Status { get; set; }
            [JsonProperty("conclusion")]
            public string Conclusion { get; set; }
        }
      
        public class InstallationEventModel
        {
            [JsonProperty("id")]
            public long Id { get; set; }
        }

    }
}
