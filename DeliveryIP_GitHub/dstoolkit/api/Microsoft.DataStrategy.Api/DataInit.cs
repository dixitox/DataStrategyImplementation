using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;

namespace Microsoft.DataStrategy.Api
{
    public class DataInit : IDataInit
    {
        private readonly IAssetsRepository _assetsRepo;
        private readonly ITagRepository _tagsRepo;
        private readonly IIndustryRepository _industriesRepo;
        private readonly IStorageFacade _storage;
        private readonly ISearchIndexService _searchIndexService;
        private const string _dataDirectory = "InitData";
        private const string _tagsFile = "tags.json";
        private const string _industriesFile = "industries.json";
        private const string _assetsDirectory = "Assets";
        private readonly string _gitHubOrganizationName;
      
        public DataInit(IAssetsRepository assetsRepo, ITagRepository tagsRepo, IIndustryRepository industriesRepo, IStorageFacade storage, ISearchIndexService searchIndexService, AppConfig config)
        {
            _assetsRepo = assetsRepo;
            _tagsRepo = tagsRepo;
            _industriesRepo = industriesRepo;
            _storage = storage;
            _searchIndexService = searchIndexService;
            _gitHubOrganizationName = config.GitHubConfiguration.Organization;
        }
                        
        public async Task Init()
        {
            await _tagsRepo.InitContainerData($"{_dataDirectory}/{_tagsFile}");
            await _industriesRepo.InitContainerData($"{_dataDirectory}/{_industriesFile}");
            var assets = await _assetsRepo.InitContainerData($"{_dataDirectory}/{_assetsDirectory}", _gitHubOrganizationName);
            await ImportImages(assets);
            await _searchIndexService.RebuildIndexAsync();
        }

        private async Task ImportImages(List<Asset> assets)
        {
            foreach (var asset in assets)
            {
                var image = File.ReadAllBytes($"{_dataDirectory}/{_assetsDirectory}/{asset.Screenshot.Path}");
                await _storage.UploadFileAsync(asset.Screenshot.Path, image);
            }
        }
    }

    public interface IDataInit
    {
        public Task Init();
    }
}
