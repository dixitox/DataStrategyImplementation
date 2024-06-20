using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.DataStrategy.Tools
{
    public class AssetsImporter : IAssetsImporter
    {
        IAssetsRepository _assetsRepository;
        IAssetsService _assetsService;
        IAssetSearchService _assetSearchService;
        IStorageFacade _storage;
        AppConfig _configuration;
        string _directory;

        public AssetsImporter(IAssetsRepository assetRepository, IAssetsService assetService, IAssetSearchService assetSearchService, IStorageFacade storage, AppConfig appConfiguration, IConfiguration configuration)
        {
            _assetsRepository = assetRepository;
            _assetsService = assetService;
            _assetSearchService = assetSearchService;
            _storage = storage;
            _configuration = appConfiguration;
            _directory = configuration["ImporterDirectory"];
        }

        public async Task DeleteImportedAssets()
        {
            Console.WriteLine($"Starting deleting old imported assets...\n");
            var assets = await _assetsRepository.GetAssetsAsync(1000, 1, false);
            foreach (var proj in assets.Values.Where(p => p.ImportV1))
            {
                await DeleteAsset(proj);
            }
        }

        private async Task DeleteAsset(Asset proj)
        {
            Console.WriteLine($"\nDeleting {proj.Name}");
            try
            {
                Console.WriteLine($"Deleting file from storage {proj.Screenshot.Path}");
                await _storage.DeleteFileAsync(proj.Screenshot.Path);
            }
            catch (Exception) { }

            try
            {
                Console.WriteLine($"Deleting document from search index");
                await _assetSearchService.DeleteAssetAsync(proj.Id);
            }
            catch (Exception) { }
            Console.WriteLine($"Deleting document from cosmos db");
            await _assetsRepository.DeleteAssetAsync(proj.Id);
        }

        public async Task ImportAssets()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            foreach (var file in Directory.GetFiles(_directory))
            {
                Console.WriteLine($"Importing {file}");
                var yamlProj = deserializer.Deserialize<dynamic>(File.ReadAllText(file));
                await ImportAsset(yamlProj);
            }
        }

        private async Task ImportAsset(dynamic yamlProj)
        {
            var asset = AssetMapper.MapV1YamlToAsset(yamlProj, _configuration.Storage.AssetImagesDirectory);
            if (!string.IsNullOrEmpty(asset.Screenshot?.Path))
                await ImportImage(asset.Screenshot.Path);
            await _assetsService.CreateAssetAsync(asset, true);
        }

        public async Task ValidateData()
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            foreach (var file in Directory.GetFiles(_directory))
            {
                Console.WriteLine($"Validating {file}");
                var yamlProj = deserializer.Deserialize<dynamic>(File.ReadAllText(file));
                var asset = AssetMapper.MapV1YamlToAsset(yamlProj, _configuration.Storage.AssetImagesDirectory);
                if (!string.IsNullOrEmpty(asset.Screenshot?.Path))
                    File.ReadAllBytes($"{_directory}/{asset.Screenshot.Path}");
            }
        }

        private async Task ImportImage(string path)
        {
            var image = File.ReadAllBytes($"{_directory}/{path}");
            await _storage.UploadFileAsync(path, image);
        }

    }
}
