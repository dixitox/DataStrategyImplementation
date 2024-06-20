namespace Microsoft.DataStrategy.Tools
{
    public interface IAssetsImporter
    {
        public Task DeleteImportedAssets();
        public Task ImportAssets();
        public Task ValidateData();
    }
}
