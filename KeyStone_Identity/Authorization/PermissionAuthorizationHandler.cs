using KeyStone_Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KeyStone_Identity.API.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private KeyStone_Identity_DbContext _DbContext;
        public PermissionAuthorizationHandler(KeyStone_Identity_DbContext dbContext)
        {
            _DbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userIDClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            if(userIDClaim is null || !long.TryParse(userIDClaim.Value, out var userID))
            {
                return;
            }

            var hasPermission = await _DbContext.UserRoles.AnyAsync(ur => ur.UserId == userID && ur.Role.RolePermissions.Any(rp => rp.Permission.Name == requirement._permission));

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
