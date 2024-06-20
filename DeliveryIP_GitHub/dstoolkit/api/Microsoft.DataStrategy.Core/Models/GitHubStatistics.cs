using Newtonsoft.Json;
using Octokit;

namespace Microsoft.DataStrategy.Core.Models
{
    public class GitHubStatistics
    {
        public GitHubStatistics() { }
        public GitHubStatistics(string id, string assetUrl)
        {
            Id = id;
            AssetUrl = assetUrl;
            LastJobExecution = DateTime.Now;
            Views = new List<WeeklyStatistic>();
            Clones = new List<WeeklyStatistic>();
            Referrers = new List<ReferrerStatistics>();
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("partitionKey")]
        public string PartitionKey => "1";
        [JsonProperty("assetUrl")]
        public string AssetUrl { get; set; }
        [JsonProperty("views")]
        public List<WeeklyStatistic> Views { get; set; }
        [JsonProperty("clones")]
        public List<WeeklyStatistic> Clones { get; set; }
        [JsonProperty("stargazers")]
        public long Stargazers { get; set; }
        [JsonProperty("subscribers")]
        public long Subscribers { get; set; }
        [JsonProperty("forks")]
        public long Forks { get; set; }
        [JsonProperty("lastPush")]
        public DateTimeOffset? LastPushed { get; set; }
        [JsonProperty("contributors")]
        public List<RepoContributor> Contributors { get; set; }
        [JsonProperty("referrals")]
        public List<ReferrerStatistics> Referrers { get; set; }        
        [JsonProperty("lastJobExecution")]
        public DateTime LastJobExecution { get; set; }
        [JsonProperty("lastReferralAggregation")]
        public DateTime? LastReferralAggregation { get; set; }
        [JsonProperty("valid")]
        public bool Valid { get; set; }
        [JsonProperty("comments")]
        public string Comments { get; set; }

    }

    public class WeeklyStatistic
    {
        public WeeklyStatistic() { }
        public WeeklyStatistic(RepositoryTrafficView view)
        {
            Week = view.Timestamp;
            Uniques = view.Uniques;
            Count = view.Count;
        }

        public WeeklyStatistic(RepositoryTrafficClone clone)
        {
            Week = clone.Timestamp;
            Uniques = clone.Uniques;
            Count = clone.Count;
        }

        [JsonProperty("week")]
        public DateTimeOffset Week { get; set; }
        [JsonProperty("count")]
        public long Count { get; set; }
        [JsonProperty("uniques")]
        public long Uniques { get; set; }
    }

    public class ReferrerStatistics
    {
        public ReferrerStatistics() { }
        public ReferrerStatistics(RepositoryTrafficReferrer referrer)
        {
            Url = referrer.Referrer;
            Uniques = referrer.Uniques;
            Count = referrer.Count;
        }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("count")]
        public long Count { get; set; }
        [JsonProperty("uniques")]
        public long Uniques { get; set; }
    }

    public class RepoContributor
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        [JsonProperty("contributions")]
        public int Contributions { get; set; }
    }


}