namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IBookmarksService
    {
        public Task<List<string>> GetUserBookmarksAsync(string userId);
        public Task AddBookmarkAsync(string userId, string assetId);
        public Task RemoveBookmarkAsync(string userId, string assetId);

    }
}
