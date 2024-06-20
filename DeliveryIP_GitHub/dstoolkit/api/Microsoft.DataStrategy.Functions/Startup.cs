using Azure.Identity;
using Microsoft.DataStrategy.Core.Repositories;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.DataStrategy.Core;
using System;
using Microsoft.DataStrategy.Functions.AdaptiveCards;
using System.IO;

[assembly: FunctionsStartup(typeof(Microsoft.DataStrategy.Functions.Startup))]
namespace Microsoft.DataStrategy.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();      
            var builtConfig = builder.ConfigurationBuilder.Build();
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddAzureKeyVault(new Uri($"https://{builtConfig["KeyVault"]}.vault.azure.net/"), new DefaultAzureCredential());            
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            var config = new AppConfig();
            builder.GetContext().Configuration.Bind(config);

            builder.Services.AddSingleton(config);
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IAssetsRepository, AssetsRepository>();
            builder.Services.AddSingleton<IGitHubStatisticsRepository, GitHubStatisticsRepository>();
            builder.Services.AddSingleton<IGitHubService, GitHubService>();
            builder.Services.AddSingleton<IAdaptiveCardsMapper, AdaptiveCardsMapper>();         
            builder.Services.AddSingleton<ICommunicationService, CommunicationService>();
            builder.Services.AddSingleton((s) =>
            {
                return new CosmosClientBuilder(config.Cosmos.Endpoint, new DefaultAzureCredential())
                    .Build();
            });



            builder.Services.AddSingleton((s) =>
            {
                return new CosmosClientBuilder(config.Cosmos.Endpoint, new DefaultAzureCredential())
                    .WithBulkExecution(true)
                    .Build();
            });
        }
    }
}
