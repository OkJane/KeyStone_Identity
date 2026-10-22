using Microsoft.AspNetCore.Authorization;

namespace KeyStone_Identity.API.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string _permission;
        public PermissionRequirement(string permission)
        {
            _permission = permission;
        }
    }
}
