using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IBookmarksRepository
    {
        public Task<UserBookmarks> GetUserBookmarksAsync(string id);
        public Task DeleteUserBookmarksAsync(string id);
        public Task UpsertUserBookmarksAsync(UserBookmarks asset);
    }
}

