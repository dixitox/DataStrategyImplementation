using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.DataStrategy.Core.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlogEntryType
    {
        [EnumMember(Value = "announcement")]
        Announcement,
        [EnumMember(Value = "guidance")]
        Guidance
    }
}
