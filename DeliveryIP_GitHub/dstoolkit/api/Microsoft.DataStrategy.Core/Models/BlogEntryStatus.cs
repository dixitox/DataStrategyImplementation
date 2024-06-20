using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Microsoft.DataStrategy.Core.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlogEntryStatus
    {
        [EnumMember(Value = "draft")]
        Draft,
        [EnumMember(Value = "published")]
        Published
    }
}
