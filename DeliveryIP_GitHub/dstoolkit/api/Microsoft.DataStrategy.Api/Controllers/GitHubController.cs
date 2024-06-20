using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Api.Models;
using Microsoft.DataStrategy.Core.Extensions;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Services.Interfaces;

namespace Microsoft.DataStrategy.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class GitHubController : CustomControllerBase
    {
        private readonly ILogger<GitHubController> _logger;
        private readonly IAssetsService _assetService;
        private readonly IGitHubDeploymentsService _gitHubDeploymentsService;        
        
        public GitHubController(ILogger<GitHubController> logger, IGitHubDeploymentsService gitHubDeploymentsService, IAssetsService assetService)
        {
            _logger = logger;
            _gitHubDeploymentsService = gitHubDeploymentsService;
            _assetService = assetService;
        }

        [HttpPost("WebHook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> WebHookEventReceivedAsync(WorkflowRunGitHubWebhook payload)
        {
            if (!HttpContext.Request.Headers.TryGetValue("X-Hub-Signature-256", out var signature))
                return Forbid();

            if (!HttpContext.Request.Headers.TryGetValue("X-GitHub-Event", out var githubEvent) || !githubEvent.Equals("workflow_run"))
                return Forbid();

            if (!await _gitHubDeploymentsService.IsSignatureValid(Request.Body, signature))
                return Forbid();

            await _gitHubDeploymentsService.ProcessWebhookEventAsync(payload, "");
            return Ok();
        }        

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpPost("/Asset/{assetId}/WorkflowDispatch/{environment}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> WorkflowDispatchAsync(string assetId, DeployEnvironmentEnum environment)
        {
            var asset = await _assetService.GetAssetAsync(assetId);

            if (asset == null || string.IsNullOrEmpty(asset.WorkflowUrl))
                return NotFound();
                        
            var result = await _gitHubDeploymentsService.DeployEnvironment(asset, environment, GetUserActionMetadata());
            if (result)
                return Ok();
            else
                return BadRequest();
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpGet("/Asset/{assetId}/EnvironmentsStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DeploymentsStatusModel>> EnvironmentsStatusAsync(string assetId)
        {
           var asset = await _assetService.GetAssetAsync(assetId);
           
            if (asset == null || string.IsNullOrEmpty(asset.WorkflowUrl))
                return NotFound();

            var status = await _gitHubDeploymentsService.GetDeploymentStatusAsync(GitHubHelpers.GetRepoNameFromRepoUrl(asset.RepositoryUrl));
            
            if (status == null)
            {
                var emptyModel = new DeploymentsStatusModel();
                return Ok(emptyModel);
            }

            var model = new DeploymentsStatusModel()
            {
                Sandbox = new EnvironmentModel
                {
                    IsDeployable = _gitHubDeploymentsService.IsEnvironmentDeployable(status, DeployEnvironmentEnum.Sandbox),
                    LastStatus = status.Environments.FirstOrDefault(x => x.EnvironmentName == DeployEnvironmentEnum.Sandbox)?.LastStatus ?? DeployStatusEnum.None
                },
                Production = new EnvironmentModel
                {
                    IsDeployable = _gitHubDeploymentsService.IsEnvironmentDeployable(status, DeployEnvironmentEnum.Production),
                    LastStatus = status.Environments.FirstOrDefault(x => x.EnvironmentName == DeployEnvironmentEnum.Production)?.LastStatus ?? DeployStatusEnum.None
                }
            };

            return Ok(model);
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpGet("/Asset/{assetId}/DeploymentHistory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DeploymentHistory>>> GetDeploymentHistory(string assetId, [FromQuery] string[] environments)
        {
            var asset = await _assetService.GetAssetAsync(assetId);

            if (asset == null)
                return NotFound();

            var deploymentHistory = await _gitHubDeploymentsService.GetDeploymentHistories(GitHubHelpers.GetRepoNameFromRepoUrl(asset.RepositoryUrl), environments);
            return Ok(deploymentHistory);

        }

    }
}