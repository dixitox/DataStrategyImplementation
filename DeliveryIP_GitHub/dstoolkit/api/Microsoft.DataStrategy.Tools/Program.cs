using Azure.Identity;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Repositories;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.DataStrategy.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false);
var conf = builder.Build();
var appConfig = new AppConfig();
conf.Bind(appConfig);

var services = new ServiceCollection()
     .AddLogging(opts =>
     {
         opts.AddConsole();
     })
     .AddSingleton<IConfiguration>(conf)
     .AddSingleton(appConfig)
     .AddSingleton<IAssetsRepository, AssetsRepository>()
     .AddSingleton<IAssetsService, AssetsService>()
     .AddSingleton<IAssetSearchService, AssetSearchService>()
     .AddSingleton<ISearchIndexService, SearchIndexService>()
     .AddSingleton<IStorageFacade, StorageFacade>()
     .AddSingleton<IAssetsImporter, AssetsImporter>()
     .AddSingleton((s) =>
     {
         return new CosmosClientBuilder(appConfig.Cosmos.Endpoint, new DefaultAzureCredential())
        .Build();
     });

var app = builder.Build();
var serviceProvider = services.BuildServiceProvider();
var importer = serviceProvider.GetService<IAssetsImporter>();
var searchIndexService = serviceProvider.GetService<ISearchIndexService>();
//var appConfig = serviceProvider.GetService<AppConfig>();

Console.WriteLine("Welcome to Data Strategy Manager Console\n\n");
Console.WriteLine("Key 1 - Asset importer");
Console.WriteLine("Key 2 - Rebuild search index");
Console.WriteLine("\nPlease select the operation...");

var menu = Console.ReadKey();


switch (menu.KeyChar)
{
    case '1':
        await ExecuteImporter();
        break;
    case '2':
        await ExecuteRebuildSearchIndex();
        break;
    default:
        break;
}


async Task ExecuteImporter()
{
    Console.Clear();
    Console.WriteLine("Asset importer\n\n");
    Console.WriteLine("Validating data...\n\n");
    await importer.ValidateData();
    Console.WriteLine("Yaml in folder are valid\n\n");

    Console.WriteLine("Do you want do delete old imported assets? (Y/N)");
    var response = Console.ReadKey();
    Console.Clear();

    if (response.KeyChar == 'Y' || response.KeyChar == 'y')
        await importer.DeleteImportedAssets();

    Console.WriteLine($"\nStarting import of YAML assets in folder\n");
    await importer.ImportAssets();
    await searchIndexService.RunIndexerAsync();
}

async Task ExecuteRebuildSearchIndex()
{
    Console.Clear();
    Console.WriteLine($"This process will:");
    Console.WriteLine($"DELETE index {appConfig.Search.AssetsIndex} from {appConfig.Search.Endpoint}");
    Console.WriteLine($"CREATE updated index {appConfig.Search.AssetsIndex}");
    Console.WriteLine($"RESET indexer {appConfig.Search.AssetsIndexer}");
    Console.WriteLine($"RUN indexer {appConfig.Search.AssetsIndexer}");
    Console.WriteLine($"\nContinue? (Y/N)");

    var response = Console.ReadKey();
    Console.Clear();

    if (response.KeyChar == 'Y' || response.KeyChar == 'y')
        await searchIndexService.RebuildIndexAsync();
}