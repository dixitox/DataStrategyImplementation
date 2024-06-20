using Microsoft.DataStrategy.Core.Models;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Api.Models
{
    public class DeploymentsStatusModel
    {
        [JsonProperty("sandbox")]
        public EnvironmentModel Sandbox { get; set; }
        [JsonProperty("production")]
        public EnvironmentModel Production { get; set; }

        public DeploymentsStatusModel()
        {
            Sandbox = new EnvironmentModel() { IsDeployable = true, LastStatus = DeployStatusEnum.None };
            Production = new EnvironmentModel() { IsDeployable = false, LastStatus = DeployStatusEnum.None };
        }
    }

    public class EnvironmentModel
    {
        [JsonProperty("isDeployable")]
        public bool IsDeployable { get; set; }
        [JsonProperty("IsDeploying")]
        public bool IsDeploying { get { return LastStatus == DeployStatusEnum.Requested || LastStatus == DeployStatusEnum.InProgress || LastStatus == DeployStatusEnum.Queued; } }
        [JsonProperty("lastStatus")]
        public DeployStatusEnum LastStatus { get; set; }
    }
}