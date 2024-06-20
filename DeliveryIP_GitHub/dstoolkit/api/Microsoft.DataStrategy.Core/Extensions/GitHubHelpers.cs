namespace Microsoft.DataStrategy.Core.Extensions
{
    public static class GitHubHelpers
    {
        public static string GetOrganizationFromRepoUrl(string url)
        {
            if (!IsValidGitHubRepo(url))
            {
                return string.Empty;
            }
            
            var urlTokens = url.Replace("https://", "").Replace("http://", "").Split("/");

            return urlTokens[1].ToLower();
        }
        public static string GetRepoNameFromRepoUrl(string url)
        {
            if (!IsValidGitHubRepo(url))
            {
                return string.Empty;
            }

            var urlTokens = url.Replace("https://", "").Replace("http://", "").Split("/");

            return urlTokens[2].ToLower();
        }

        public static string GetWorkflowNameFromUrl(string url)
        {
            if (!IsValidGitHubRepo(url))
            {
                return string.Empty;
            }

            var urlTokens = url.Replace("https://", "").Replace("http://", "").Split("/");

            return urlTokens.Last().ToLower();
        }

        public static bool IsValidGitHubRepo(string url)
        {
            var urlTokens = url.Replace("https://", "").Replace("http://", "").Split("/");

            if (urlTokens.Length < 3 || !urlTokens[0].ToLower().Contains("github.com"))
            {
                return false;
            }

            return true;
        }

        public static bool IsGitHubRepoRoot(string url)
        {
            var urlTokens = url.Replace("https://", "").Replace("http://", "").Split("/");

            if (urlTokens.Length != 3 || !urlTokens[0].ToLower().Contains("github.com"))
            {
                return false;
            }

            return true;
        }
    }
}
