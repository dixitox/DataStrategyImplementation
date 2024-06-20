namespace Microsoft.DataStrategy.Core.Models.ServiceBusModels
{
    public class AssetOperationMessage
    {
        public AssetOperationMessage() { }
        public AssetOperationMessage(AssetOperation op, Asset asset, UserActionMetadata userInfo, Asset oldAsset = null) {
            Operation = op;
            Asset = asset;
            UserMetadata = userInfo;
            OldAsset = oldAsset;
        }
        
        public AssetOperation Operation { get; set; }
        public Asset Asset { get; set; }
        public Asset OldAsset { get; set; }
        public UserActionMetadata UserMetadata { get; set; }

    }
}
