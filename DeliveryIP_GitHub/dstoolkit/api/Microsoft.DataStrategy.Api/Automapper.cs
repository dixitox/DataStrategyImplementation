using AutoMapper;
using Microsoft.DataStrategy.Api.Models;
using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Api
{
    public class AssetProfile : Profile
    {
        public AssetProfile()
        {
            CreateMap<CreateAssetModel, Asset>();
            CreateMap<UpdateAssetModel, Asset>();
            CreateMap<CreateBlogEntryModel, BlogEntry>();
            CreateMap<PendingAssetReview, AssetReview>();
            CreateMap<UserActionMetadata, IndexedUserMetadata>();
        }
    }

}
