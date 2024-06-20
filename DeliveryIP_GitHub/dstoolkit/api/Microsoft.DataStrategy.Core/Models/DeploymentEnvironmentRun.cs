using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DataStrategy.Core.Models
{
    public class DeploymentEnvironmentRun
    {
        [JsonProperty("runId")]
        public long RunId { get; set; }

        [JsonProperty("lastUpdatedBy")]
        public UserActionMetadata LastUpdatedBy { get; set; }
    }

    public class DeploymentEnvironmentHistory 
    {
        [JsonProperty("RunHistory")]
        public List<DeploymentEnvironmentRun> RunHistory { get; set; }
    }
}
