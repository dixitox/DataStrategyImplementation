namespace Microsoft.DataStrategy.Core.Facades
{
    public interface IStorageFacade
    {
        public Task UploadFileAsync(string filePath, byte[] content);
        public Task UploadFileFromUrlAsync(string filePath, string url);
        public Task DeleteFileAsync(string filePath);
        public Task<List<string>> ListFilesAsync(string path);        

    }
}
