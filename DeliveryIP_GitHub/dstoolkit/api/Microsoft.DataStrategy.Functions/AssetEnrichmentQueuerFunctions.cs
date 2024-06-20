using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.DataStrategy.Functions
{
    public class AssetEnrichmentQueuerFunctions
    {
        private IAssetsRepository _assetRepository;

        public AssetEnrichmentQueuerFunctions(IAssetsRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }              

        [FunctionName("AssetsEnrichmentQueuer")]
        public async Task AssetsEnrichmentQueuer(
        [TimerTrigger("%AssetsEnrichmentQueuerTriggerTime%")] TimerInfo myTimer,
        ILogger log, [ServiceBus("%AssetsEnrichmentToExecuteQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<PendingAssettMessage> dispatcher)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var assets = await _assetRepository.GetAssetsAsync(100, 1, true);
            var messages = assets.Values.Select(x => new PendingAssettMessage { Id = x.Id, GitHubUrl = x.RepositoryUrl });
            foreach (var message in messages)
            {
                await dispatcher.AddAsync(message);
            }
        }

        [FunctionName("AssetsEnrichmentManualQueuer")]
        public async Task AssetsEnrichmentManualQueuer(
       [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log, [ServiceBus("%AssetsEnrichmentToExecuteQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<PendingAssettMessage> dispatcher)
        {
            log.LogInformation($"C# HTTP trigger function executed at: {DateTime.Now}");
            var assets = await _assetRepository.GetAssetsAsync(100, 1, true);
            var messages = assets.Values.Select(x => new PendingAssettMessage { Id = x.Id, GitHubUrl = x.RepositoryUrl });
            foreach (var message in messages)
            {
                await dispatcher.AddAsync(message);
            }
        }

    }
}
