using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core
{
    public record AppConfig()
    {
        public AzureAd AzureAd { get; set; }
        public Cosmos Cosmos { get; set; }
        public Search Search { get; set; }
        public AzureStorage Storage { get; set; }
        public CachingPolicies CachingPolicies { get; set; }
        public GitHubConfiguration GitHubConfiguration { get; set; }
        public ServiceBusConnection ServiceBusConnection { get; set; }
        public TeamsConfiguration Teams { get; set; }  
        public CommunicationServiceConfiguration CommunicationServiceConfiguration { get; set; }
        public string AppUrl { get; set; }
        public bool AllowAnonymousUser { get; set; }
        public bool LoggedInUserAsConsumer { get; set; }

    }

    public record CachingPolicies
    {
        public bool Enabled { get; set; }
        public CacheProfile Profile { get; set; }
    }

    public record GitHubConfiguration
    {
        public long AppId { get; set; }
        public long InstallationId { get; set; }
        public string PEMKey { get; set; }
        public string Token { get; set; }
        public int TokenExpiration { get; set; }
        public string Organization { get; set; }
        public bool UsePAT { get; set; }
        public string Secret { get; set; }
    }

    public record AzureAd
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string[] Scopes { get; set; }
        public string CallbackPath { get; set; }
    }

    public record Cosmos
    {
        public string Endpoint { get; set; }
        public string Database { get; set; }
        public string AssetsContainer { get; set; }
        public string BookmarksContainer { get; set; }        
        public string TagsContainer { get; set; }
        public string IndustriesContainer { get; set; }
        public string GitHubStatisticsContainer { get; set; }
        public string PendingReviewsContainer { get; set; }
        public string ReviewsContainer { get; set; }
        public string DeploymentsStatusContainer { get; set; }
        public string BlogEntriesContainer { get; set; }
    }

    public record Search
    {
        public string Endpoint { get; set; }
        public string AssetsIndex { get; set; }
        public string AssetsIndexer { get; set; }
    }
    
    public record AzureStorage
    {
        public string AccountName { get; set; }
        public string ContainerName { get; set; }
        public string AssetImagesDirectory { get; set; }
        public string BlogImagesDirectory { get; set; }        
    }

    public record ServiceBusConnection
    {
        [JsonProperty("fullyQualifiedNamespace")]
        public string FullyQualifiedNamespace { get; set; }
        public string AssetChangeNotificationQueue { get; set; }
        public string AssetReviewNotificationQueue { get; set; }
        public string MailQueue { get; set; }
    }
    public record TeamsConfiguration
    {
        public string MainIncomingWebHook { get; set; }
        public string ImagePath { get; set; }
        public string UserDeepLink { get; set; }
    }

    public record CommunicationServiceConfiguration
    {
        public string ConnectionString { get; set; }
        public string EmailSender { get; set; }
        public string PlatformContactRecipients { get; set; }

    }

}
