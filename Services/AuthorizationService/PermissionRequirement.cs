using Microsoft.AspNetCore.Authorization;

namespace AngularAdminPannel.Services.AuthorizationService
{
    public class PermissionRequirement:IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
