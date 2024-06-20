using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.DataStrategy.Core.Services
{
    public class AssetsService : IAssetsService
    {
        private readonly IStorageFacade _storageFacade;
        private readonly IAssetsRepository _assetRepository;
        private readonly ServiceBusSender _serviceBusSender;        
        private ILogger<AssetsService> _logger;
        private readonly string _assetsImgDirectory;
        
        public AssetsService(IStorageFacade storageFacade, IAssetsRepository assetRepository, ILogger<AssetsService> logger, AppConfig config)
        {
            _storageFacade = storageFacade;
            _assetRepository = assetRepository;
            _assetsImgDirectory = config.Storage.AssetImagesDirectory;
            var serviceBusClient = new ServiceBusClient(config.ServiceBusConnection.FullyQualifiedNamespace, new DefaultAzureCredential());
            _serviceBusSender = serviceBusClient.CreateSender(config.ServiceBusConnection.AssetChangeNotificationQueue);
            _logger = logger;
        }

        public async Task<Asset?> GetAssetAsync(string id) => await _assetRepository.GetAssetAsync(id);
        public async Task<PagedResult<AssetBase>> GetAssetsBaseAsync(int pageSize, int page, bool? enabled, string? orderBy, bool descending = true, string filterByUser = "") => await _assetRepository.GetAssetsBaseAsync(pageSize, page, enabled, orderBy, descending, filterByUser);
        
        public async Task CreateAssetAsync(Asset asset, bool enabled = false)
        {           
            asset.Enabled = enabled;
            asset.Order = await _assetRepository.GetMaxOrder() + 1;
            await UpsertAssetAsync(asset);
            await SendNotificationAsync(new AssetOperationMessage(AssetOperation.Created, asset, asset.CreatedBy));
        }

        public async Task UpdateAssetAsync(Asset asset)
        {            
            await UpsertAssetAsync(asset);
            await SendNotificationAsync(new AssetOperationMessage(AssetOperation.Updated, asset, asset.LastChangedBy));
        }

        private async Task UpsertAssetAsync(Asset asset)
        {          
            if (!string.IsNullOrEmpty(asset.Screenshot?.Base64))
            {
                var contentType = asset.Screenshot.Base64.Substring(0, asset.Screenshot.Base64.IndexOf(';')).Replace("data:", "");
                var content = Convert.FromBase64String(asset.Screenshot.Base64.Split(',')[1]);
                asset.Screenshot.Path = $"{_assetsImgDirectory}/{asset.Id}.{GetFileExtensionFromContentType(contentType)}";
                await _storageFacade.UploadFileAsync(asset.Screenshot.Path, content);
            }

            await _assetRepository.UpsertAssetAsync(asset);
        }

        public async Task DeleteAssetAsync(string id, UserActionMetadata userInfo)
        {
            var asset = await _assetRepository.GetAssetAsync(id);
            if (asset == null) return;         

            if (asset.Screenshot != null)
            {
                await _storageFacade.DeleteFileAsync(asset.Screenshot.Path);
            }

            await _assetRepository.DeleteAssetAsync(id);
            await SendNotificationAsync(new AssetOperationMessage(AssetOperation.Deleted, asset, userInfo));
        }

        private string GetFileExtensionFromContentType(string contentType)
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"image/bmp", "bmp"},
                {"image/jpeg", "jpg"},
                {"image/png", "png"}, 
                {"image/x-png", "png"}
                };
            return mappings[contentType];
        }

        private async Task SendNotificationAsync(AssetOperationMessage message)
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }

    }
}
