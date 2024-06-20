using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.DataStrategy.Core.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeployEnvironmentEnum
    {
        [EnumMember(Value = "sandbox")]
        Sandbox,
        [EnumMember(Value = "production")]
        Production
    }
}
