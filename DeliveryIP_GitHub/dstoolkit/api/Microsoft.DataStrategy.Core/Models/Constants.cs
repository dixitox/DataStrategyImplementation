namespace Microsoft.DataStrategy.Core.Models
{
    public static class PlatformRoles
    {
        public const string Admin = "PlatformAdmin";
        public const string Producer = "PlatformProducer";
        public const string Consumer = "PlatformConsumer";
        public const string AdminOrProducer = Admin + "," + Producer;
    }

    public static class PlatformPolicies
    {
        public const string Consumer = "Consumer";
    }

    public static class AssetType
    {
        public const string Accelerator = "Accelerator";
        public const string Demo = "Demo";
    }
}
