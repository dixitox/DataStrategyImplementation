using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Graph;
using Microsoft.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using User = Microsoft.Graph.User;
using Microsoft.AspNetCore.Http;

namespace Microsoft.DataStrategy.Core.Services
{
    public class GraphService : IGraphService
    {
        private readonly IConfidentialClientApplication _clientApp;
        private readonly string _userToken;
        private readonly string[] graphScopes = { "https://graph.microsoft.com/User.Read" };

        public GraphService(AppConfig appconfig, IHttpContextAccessor httpContextAccessor)
        {
            _clientApp = ConfidentialClientApplicationBuilder
                   .Create(appconfig.AzureAd.ClientId)
                   .WithAuthority($"https://login.microsoftonline.com/{appconfig.AzureAd.TenantId}/oauth2/v2.0")
                   .WithClientSecret(appconfig.AzureAd.Secret)
                   .Build();
            _userToken = httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
        }

        private async Task<GraphServiceClient> GetGraphClientAsync()
        {
            if (true)
                return await GetDelegatedGraphClientAsync();

            return await GetApplicationGraphClientAsync();
        }

        private async Task<GraphServiceClient> GetApplicationGraphClientAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://graph.microsoft.com/");
            return await GetGraphClientAsync(accessToken);
        }

        private async Task<GraphServiceClient> GetDelegatedGraphClientAsync()
        {
            UserAssertion userAssertion = new UserAssertion(_userToken);
            var parameterBuilder = _clientApp.AcquireTokenOnBehalfOf(graphScopes, userAssertion);
            await parameterBuilder.ExecuteAsync();
            var graphToken = await _clientApp.AcquireTokenOnBehalfOf(graphScopes, userAssertion).ExecuteAsync();
            return await GetGraphClientAsync(graphToken.AccessToken);
        }

        private async Task<GraphServiceClient> GetGraphClientAsync(string token)
        {
            var graphServiceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage
                .Headers
                .Authorization = new AuthenticationHeaderValue("bearer", token);

                    return Task.CompletedTask;
                }));
            return graphServiceClient;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="upn"></param>
        /// <param name="token"></param>
        /// <param name="pixel"> 48, 64, 96, 120, 240, 360, 432, 504, 648.</param>
        /// <returns></returns>
        public async Task GetGroups(string userId)
        {
            var graphClient = await GetGraphClientAsync();
            var groups = await graphClient.Users[userId].MemberOf.Request().GetAsync();
            var checkMemberGroups = await graphClient.Users[userId].CheckMemberGroups(new List<string> { "cdee1719-7812-460f-9f40-d10d9a2bd5bd", "af64cc59-51af-41b4-811c-5816d8428a5f", "3fd3f227-7d4b-4f71-8782-a9be33d13b1b", "0624ea88-ac42-4f4d-8fdc-50d9e55f0d38" }).Request().PostAsync();
            return;
                //.Header("ConsistencyLevel", "eventual").GetAsync();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="upn"></param>
        /// <param name="token"></param>
        /// <param name="pixel"> 48, 64, 96, 120, 240, 360, 432, 504, 648.</param>
        /// <returns></returns>
        public async Task<Stream> GetImage(string upn, int pixel)
        {
            var graphClient = await GetGraphClientAsync();
            return await GetImage(graphClient, upn, pixel);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="upn"></param>
        /// <param name="token"></param>
        /// <param name="pixel"> 48, 64, 96, 120, 240, 360, 432, 504, 648.</param>
        /// <returns></returns>
        public async Task<Stream> GetImageByEmailOrName(string userDisplayName, string email, int pixel)
        {
            var graphClient = await GetGraphClientAsync();
            Stream imageStream = default;
            if (!string.IsNullOrEmpty(email))
                imageStream = await GetImage(graphClient, email, pixel);

            if (imageStream != default)
                return imageStream;

            var user = await SearchUserByName(graphClient, userDisplayName);

            if (user == null)
                return default;

            return await GetImage(graphClient, user.UserPrincipalName, pixel);
        }

        private async Task<User> SearchUserByName(GraphServiceClient graphClient, string userDisplayName)
        {
            var queryOptions = new List<QueryOption>() { new QueryOption("$search", $"\"displayName:{userDisplayName}\"") };
            var userSearchResult = await graphClient.Users.Request(queryOptions).Header("ConsistencyLevel", "eventual").GetAsync();
            return userSearchResult?.FirstOrDefault();
        }

        private async Task<Stream> GetImage(GraphServiceClient graphClient, string upn, int pixel)
        {
            try
            {
                return await graphClient.Users[upn].Photos[$"{pixel}x{pixel}"].Content.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Not found is not an error since some users have no photo
                return default;
            }
        }

    }
}
