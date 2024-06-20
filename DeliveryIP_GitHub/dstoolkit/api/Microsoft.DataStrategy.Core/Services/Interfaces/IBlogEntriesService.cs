using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IBlogEntriesService
    {     
        public Task<BlogEntry?> GetBlogEntryAsync(string id);
        public Task<PagedResult<BlogEntryBase>> GetBlogEntriesBaseAsync(int pageSize, int page, bool? onlyPublished, string? orderBy, bool descending = true);

        public Task CreateBlogEntryAsync(BlogEntry entry);
        public Task<string> CreateBlogEntryImageAsync(string base64);
        public Task<List<string>> GetBlogEntryImagesAsync();
        public Task DeleteBlogEntryImageAsync(string name);
        public Task UpdateBlogEntryAsync(BlogEntry entry);
        public  Task DeleteBlogEntryAsync(string id, UserActionMetadata userInfo);

    }
}
