using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Core.Repositories.Interfaces
{
    public interface IIndustryRepository
    {
        Task<Industry> GetIndustryByIdAsync(string id);
        Task<List<string>> GetIndustriesAsync();
        Task UpsertIndustryAsync(string tag);
        Task DeleteIndustryAsync(string id);
        Task InitContainerData(string file);
    }
}

