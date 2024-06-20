using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag> GetTagByIdAsync(string id);
        Task<List<string>> GetTagsAsync();
        Task UpsertTagAsync(string tag);
        Task DeleteTagAsync(string id);
        Task InitContainerData(string file);
    }
}

