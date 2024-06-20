using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IBlogEntriesRepository
    {
        public Task<BlogEntry?> GetBlogEntryAsync(string id);
        public Task DeleteBlogEntryAsync(string id);
        public Task<PagedResult<BlogEntryBase>> GetBlogEntryBaseAsync(int pageSize, int page, bool? onlyPublished, string? orderBy, bool descending = true); 
        public  Task UpsertBlogEntryAsync(BlogEntry entry);
    }
}

