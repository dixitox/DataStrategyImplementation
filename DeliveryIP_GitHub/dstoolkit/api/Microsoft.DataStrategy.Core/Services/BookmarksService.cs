using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.DataStrategy.Core.Services
{
    public class BookmarksService : IBookmarksService
    {
        private readonly IBookmarksRepository _bookmarksRepository;
        private readonly IAssetsRepository _assetsRepository;
        private ILogger<BookmarksService> _logger;

        public BookmarksService(IBookmarksRepository bookmarksRepository, IAssetsRepository assetRepository, ILogger<BookmarksService> logger)
        {
            _bookmarksRepository = bookmarksRepository;
            _assetsRepository = assetRepository;
            _logger = logger;
        }
        public async Task<List<string>> GetUserBookmarksAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId is null");
            }

            var userBookmarks = await _bookmarksRepository.GetUserBookmarksAsync(userId);
            return userBookmarks?.BookmarkedAssets ?? new List<string>();
        }

        public async Task AddBookmarkAsync(string userId, string assetId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException("userId or assetId is null");
            }

            var userBookmarks = await _bookmarksRepository.GetUserBookmarksAsync(userId);

            if (userBookmarks == null)
                userBookmarks = new UserBookmarks(userId);

            if (userBookmarks.BookmarkedAssets.Contains(assetId))
                return;
            
            userBookmarks.BookmarkedAssets.Add(assetId);
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            asset.Bookmarked++;
            await _assetsRepository.UpsertAssetAsync(asset);
            await _bookmarksRepository.UpsertUserBookmarksAsync(userBookmarks);
        }

        public async Task RemoveBookmarkAsync(string userId, string assetId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException("userId or assetId is null");
            }

            var userBookmarks = await _bookmarksRepository.GetUserBookmarksAsync(userId);
            if (userBookmarks == null || !userBookmarks.BookmarkedAssets.Contains(assetId))
                return;                                 
            
            userBookmarks.BookmarkedAssets.Remove(assetId);
            var asset = await _assetsRepository.GetAssetAsync(assetId);
            asset.Bookmarked--;
            await _assetsRepository.UpsertAssetAsync(asset);

            if (!userBookmarks.BookmarkedAssets.Any())
                await _bookmarksRepository.DeleteUserBookmarksAsync(userId);
            else
                await _bookmarksRepository.UpsertUserBookmarksAsync(userBookmarks);
        }

    }
}
