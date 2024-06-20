using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.Net;
using static System.Reflection.Metadata.BlobBuilder;

namespace Microsoft.DataStrategy.Core.Facades
{
    public class StorageFacade : IStorageFacade
    {
        private readonly BlobContainerClient _client;

        public StorageFacade(AppConfig config)
        {
            var blobClient = new BlobServiceClient(new Uri($"https://{config.Storage.AccountName}.blob.core.windows.net/"), new DefaultAzureCredential());            
            _client = blobClient.GetBlobContainerClient(config.Storage.ContainerName);
        }

        public async Task UploadFileAsync(string filePath, byte[] content)
        {
            using var ms = new MemoryStream(content);
            var blobClient = _client.GetBlobClient(filePath);
            await blobClient.UploadAsync(ms, overwrite: true);
        }

        public async Task UploadFileFromUrlAsync(string filePath, string url)
        {
            using (WebClient client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(new Uri(url));
                await UploadFileAsync(filePath, data);
            }
        }
        
        public async Task<List<string>> ListFilesAsync(string path)
        {
            List<BlobItem> blobList = new List<BlobItem>();
            IAsyncEnumerator<BlobItem> enumerator = _client.GetBlobsAsync(BlobTraits.None, BlobStates.None, path).GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    blobList.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            
            return blobList.Select(x => x.Name).ToList();
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await _client.DeleteBlobIfExistsAsync(filePath);
        }

    }
}
