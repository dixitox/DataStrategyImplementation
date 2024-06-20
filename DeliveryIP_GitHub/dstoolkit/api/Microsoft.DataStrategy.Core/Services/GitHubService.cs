using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Microsoft.DataStrategy.Core.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubConfiguration _gitHubConfiguration;
        private GitHubClient _client;
        private DateTimeOffset _tokenExpiration;
        
        public GitHubService(AppConfig config)
        {
            _gitHubConfiguration = config.GitHubConfiguration;
        }

        public async Task<GitHubClient> GetClientAsync()
        {
            if (_client != null && _tokenExpiration != null && _tokenExpiration > DateTime.UtcNow)
                return _client;

            if (_gitHubConfiguration.UsePAT)
                return GetPATClient();

            return await GetGitHubAppAuthenticatedClient();
        }


        private GitHubClient GetPATClient()
        {
            _client = new GitHubClient(new ProductHeaderValue(_gitHubConfiguration.Organization));
            _client.Credentials = new Credentials(_gitHubConfiguration.Token);
            return _client;
        }

        private async Task<GitHubClient> GetGitHubAppAuthenticatedClient()
        {
            var installationClient = new GitHubClient(new ProductHeaderValue(_gitHubConfiguration.Organization));
            var jwtToken = CreateEncodedJwtToken(_gitHubConfiguration.TokenExpiration, _gitHubConfiguration.AppId, _gitHubConfiguration.PEMKey);
            installationClient.Credentials = new Credentials(jwtToken, AuthenticationType.Bearer);
            var accessToken = await installationClient.GitHubApps.CreateInstallationToken(_gitHubConfiguration.InstallationId);
            _client = new GitHubClient(new ProductHeaderValue(_gitHubConfiguration.Organization));
            _client.Credentials = new Credentials(accessToken.Token);
            _tokenExpiration = accessToken.ExpiresAt;
            return _client;
        }        

        private string CreateEncodedJwtToken(int expirationSeconds, long appId, string pem)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(pem), out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };
            var claims = new List<Claim>() { new Claim("iat", DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) };
            var token = new JwtSecurityToken(
                issuer: appId.ToString(),
                expires: DateTime.UtcNow.AddSeconds(expirationSeconds),
                signingCredentials: signingCredentials,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
