using Azure.Core.Serialization;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.DataStrategy.Core.Services
{
    public class SearchIndexService : ISearchIndexService
    {
        private SearchIndexClient _searchIndexClient { get; set; }
        private SearchIndexerClient _searchIndexerClient { get; set; }
        private string _indexName { get; set; }
        private string _indexerName { get; set; }
        private ObjectSerializer _serializer {get;set;}

        public SearchIndexService(AppConfig config)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _serializer = new NewtonsoftJsonObjectSerializer(serializerSettings);
            var options = new SearchClientOptions { Serializer = _serializer };
            _indexName = config.Search.AssetsIndex;
            _indexerName = config.Search.AssetsIndexer;
            _searchIndexClient = new SearchIndexClient(new Uri(config.Search.Endpoint), new DefaultAzureCredential(), options);
            _searchIndexerClient = new SearchIndexerClient(new Uri(config.Search.Endpoint), new DefaultAzureCredential(), options);
        }


        public async Task RebuildIndexAsync()
        {
            _searchIndexClient.DeleteIndex(_indexName);
            FieldBuilder fieldBuilder = new FieldBuilder();
            fieldBuilder.Serializer = _serializer;
            var searchFields = fieldBuilder.Build(typeof(Asset));
            var searchIndex = new SearchIndex(_indexName, searchFields);
            await _searchIndexClient.CreateOrUpdateIndexAsync(searchIndex);
            await _searchIndexerClient.ResetIndexerAsync(_indexerName);
            await _searchIndexerClient.RunIndexerAsync(_indexerName);
        }

        public async Task RunIndexerAsync()
        {            
            await _searchIndexerClient.RunIndexerAsync(_indexerName);
        }
    }
}
