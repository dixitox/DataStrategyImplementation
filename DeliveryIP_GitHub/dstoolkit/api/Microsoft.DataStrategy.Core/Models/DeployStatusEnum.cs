using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.DataStrategy.Core.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeployStatusEnum
    {
        [EnumMember(Value = "none")]
        None,   
        [EnumMember(Value = "requested")]
        Requested,
        [EnumMember(Value = "queued")]
        Queued,
        [EnumMember(Value = "in_progress")]
        InProgress,
        [EnumMember(Value = "completed")]
        Completed,
        [EnumMember(Value = "failure")]
        Failure
    }
}
