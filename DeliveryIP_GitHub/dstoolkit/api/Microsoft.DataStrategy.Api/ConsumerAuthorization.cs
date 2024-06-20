using Microsoft.AspNetCore.Authorization;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Models;

namespace Microsoft.DataStrategy.Api
{
    public class ConsumerRequirement : IAuthorizationRequirement { }

    public class ConsumerRequirementHandler : AuthorizationHandler<ConsumerRequirement>
    {
        private readonly bool _allowAnonymousUser;
        private readonly bool _loggedInUserAsConsumer;

        public ConsumerRequirementHandler(AppConfig config)
        {
            _allowAnonymousUser = config.AllowAnonymousUser;
            _loggedInUserAsConsumer = config.LoggedInUserAsConsumer;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ConsumerRequirement requirement)
        {
            if (_allowAnonymousUser ||
                (_loggedInUserAsConsumer && context.User.Identity.IsAuthenticated) ||
                context.User.IsInRole(PlatformRoles.Consumer) ||
                context.User.IsInRole(PlatformRoles.Admin) ||
                context.User.IsInRole(PlatformRoles.Producer))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
