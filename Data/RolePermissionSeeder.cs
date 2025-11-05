using System.Security.Claims;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Data
{
    public class RolePermissionSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        {
            var roles = new[] { "Admin", "Manager", "User" };

            foreach (var roleName in roles)
            {

                var role = await roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    role = new ApplicationRole { Name = roleName };
                    await roleManager.CreateAsync(role);
                }

                var allPermissions = Permissions.GetAll();

                foreach (var permission in allPermissions)
                {
                    var claims = await roleManager.GetClaimsAsync(role);
                    if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                    }
                }
            }
        }
    }
}
