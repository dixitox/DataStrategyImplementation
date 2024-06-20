using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class TagRepository : CosmosRepositoryBase, ITagRepository
    {
        public TagRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.TagsContainer)
        {
        }

        public async Task<Tag> GetTagByIdAsync(string id) => await GetAsync<Tag>("1", id);
        public async Task<List<string>> GetTagsAsync()
        {
            var tags = await GetAllAsync<Tag>();
            return tags.Select(x => x.Name).ToList();
        }

        public async Task UpsertTagAsync(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
                await UpsertAsync(new Tag(tag));
        }
        public async Task DeleteTagAsync(string id) => await DeleteAsync<Tag>("1", id);
        
        public async Task InitContainerData(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            
            foreach (var tag in data.tags)
            {
                await UpsertTagAsync(tag.ToString());
            }
        }
    }
}

