using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.Identity.Web;

namespace Microsoft.DataStrategy.Api.Controllers
{
    public class CustomControllerBase : ControllerBase
    {
        public CustomControllerBase()
        {
        }
        
        protected UserActionMetadata GetUserActionMetadata() => new()
        {
            DisplayName = User.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
            Mail = User.GetDisplayName(),
            ObjectId = User.GetObjectId(),
            On = DateTime.UtcNow
        };

        protected bool IsUserAuthorizedForAsset(UserActionMetadata createdBy)
        {
            return User.IsInRole(PlatformRoles.Admin) || (User.IsInRole(PlatformRoles.Producer) && createdBy.ObjectId.Equals(GetUserActionMetadata().ObjectId));
        }

    }
}