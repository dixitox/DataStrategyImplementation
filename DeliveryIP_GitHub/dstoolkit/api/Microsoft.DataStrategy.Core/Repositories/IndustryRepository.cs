using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class IndustryRepository : CosmosRepositoryBase, IIndustryRepository
    {
        public IndustryRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.IndustriesContainer)
        {
        }

        public async Task<Industry> GetIndustryByIdAsync(string id) => await GetAsync<Industry>("1", id);
        public async Task<List<string>> GetIndustriesAsync()
        {
            var industries = await GetAllAsync<Industry>();
            return industries.Select(x => x.Name).ToList();
        }

        public async Task UpsertIndustryAsync(string industry)
        {   
            if(!string.IsNullOrEmpty(industry))
                await UpsertAsync(new Industry(industry));
        } 
        public async Task DeleteIndustryAsync(string id) => await DeleteAsync<Industry>("1", id);

        public async Task InitContainerData(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            dynamic data = JsonConvert.DeserializeObject(jsonString);

            foreach (var industry in data.industries)
            {
                await UpsertIndustryAsync(industry.ToString());
            }
        }
    }
}

