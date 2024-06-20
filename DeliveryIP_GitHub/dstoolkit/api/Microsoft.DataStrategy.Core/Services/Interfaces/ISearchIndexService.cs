using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface ISearchIndexService
    {
        public Task RebuildIndexAsync();
        public Task RunIndexerAsync();
    }
}
