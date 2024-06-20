using Microsoft.Azure.Cosmos;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using System.Text;

namespace Microsoft.DataStrategy.Core.Repositories
{
    public class BookmarksRepository : CosmosRepositoryBase, IBookmarksRepository
    {
        public BookmarksRepository(CosmosClient dbClient, AppConfig config)
           : base(dbClient, config, config.Cosmos.BookmarksContainer)
        {
        }    

        public async Task<UserBookmarks> GetUserBookmarksAsync(string id) => await GetAsync<UserBookmarks>("1", id);        
        public async Task DeleteUserBookmarksAsync(string id) => await DeleteAsync<Asset>("1", id);        
        public async Task UpsertUserBookmarksAsync(UserBookmarks asset) => await UpsertAsync(asset);
    }
}

