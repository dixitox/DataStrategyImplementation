using Microsoft.Graph;

namespace Microsoft.DataStrategy.Core.Services.Interfaces
{
    public interface IGraphService
    {
        public Task GetGroups(string userId);
        public Task<Stream> GetImage(string upn, int pixel);
        public Task<Stream> GetImageByEmailOrName(string userDisplayName, string email, int pixel);
    }
}
