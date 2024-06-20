using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.DataStrategy.Api;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var config = new AppConfig();
builder.Configuration.AddAzureKeyVault(new Uri($"https://{builder.Configuration["KeyVault"]}.vault.azure.net/"), new DefaultAzureCredential());
builder.Configuration.Bind(config);
builder.Services.AddSingleton(config);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PlatformPolicies.Consumer, policy => policy.AddRequirements(new ConsumerRequirement()) );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "LocalHostCORS",
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:4000", "https://127.0.0.1:4000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

builder.Services.AddControllers(
    options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        if (config.CachingPolicies.Enabled)
        {
            options.CacheProfiles.Add("ResponseCachingProfile", config.CachingPolicies.Profile);
        }
    })
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Data Strategy API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "OAuth2.0 Auth Code with PKCE",
        Name = "oauth2",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{config.AzureAd.Instance}{config.AzureAd.TenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"{config.AzureAd.Instance}{config.AzureAd.TenantId}/oauth2/v2.0/token"),
                Scopes = config.AzureAd.Scopes.ToDictionary(x => x, x => x)
            }
        }
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            config.AzureAd.Scopes 
        }
    });
});
builder.Services.AddSingleton<IAuthorizationHandler, ConsumerRequirementHandler>();
builder.Services.AddSingleton<IAssetsRepository, AssetsRepository>();
builder.Services.AddSingleton<ITagRepository, TagRepository>();
builder.Services.AddSingleton<IIndustryRepository, IndustryRepository>();
builder.Services.AddSingleton<IAssetsService, AssetsService>();
builder.Services.AddSingleton<IBookmarksRepository, BookmarksRepository>();
builder.Services.AddSingleton<IBookmarksService, BookmarksService>();
builder.Services.AddSingleton<IPendingReviewsRepository, PendingReviewsRepository>();
builder.Services.AddSingleton<IAssetReviewsRepository, AssetReviewsRepository>();
builder.Services.AddSingleton<IReviewsService, ReviewsService>();
builder.Services.AddSingleton<IAssetSearchService, AssetSearchService>();
builder.Services.AddSingleton<IStorageFacade, StorageFacade>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IGitHubDeploymentsService, GitHubDeploymentsService>();
builder.Services.AddSingleton<IDeploymentsStatusRepository, DeploymentsStatusRepository>();
builder.Services.AddSingleton<ISearchIndexService, SearchIndexService>();
builder.Services.AddSingleton<IBlogEntriesRepository, BlogEntriesRepository>();
builder.Services.AddSingleton<IBlogEntriesService, BlogEntriesService>();


builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddSingleton<IDataInit, DataInit>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton((s) =>
{
    return new CosmosClientBuilder(config.Cosmos.Endpoint, new DefaultAzureCredential())
        .Build();
});


var app = builder.Build();
app.UseAlwaysOn();

{
    using var scope = app.Services.CreateScope();
    var assetsRepo = scope.ServiceProvider.GetRequiredService<IAssetsRepository>();
    var assets = await assetsRepo.GetAssetsAsync(1,1);

    if (!assets.Values.Any())
    {
        var dataInit = scope.ServiceProvider.GetRequiredService<IDataInit>();
        await dataInit.Init();
    }
}

app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Contains("local"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Data Strategy API v1");
        c.OAuthClientId(config.AzureAd.ClientId);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });
    app.UseCors("LocalHostCORS");
}

if (config.CachingPolicies.Enabled)
{
    builder.Services.AddResponseCaching();
    app.UseResponseCaching();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
