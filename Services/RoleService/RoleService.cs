using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Roles;
using AngularAdminPannel.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularAdminPannel.Services.RoleService
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ClaimsPrincipal? _user;

        public RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _roleManager = roleManager;
            _dbContext = context;
            _user = httpContextAccessor.HttpContext?.User;
        }

        // Create new role
        public async Task<PagedResult<RoleListItemDto>> GetRolesAsync(RoleListFilterDto filter)
        {
            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
            var query = _roleManager.Roles.AsNoTracking();
            bool hasCustomOrder = false;


            //if (!string.IsNullOrEmpty(filter.Search))
            //{
            //    var s = filter.Search.Trim();
            //    query = query.Where(r => r.Name!.Contains(s) || (r.Description ?? "").Contains(s));
            //}


            if (!string.IsNullOrWhiteSpace(filter.Filter) && !string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                var searchUpper = search.ToUpperInvariant();
                var filterType = filter.Filter.Trim().ToUpperInvariant();


                if (filterType == "NAME")
                {
                    query = query.Where(u => u.Name!.Contains(searchUpper));
                }
                
                else if (filterType == "DESCRIPTION")
                {
                    query = query.Where(u => (u.Description ?? "").Contains(searchUpper));

                }
                else if (filterType == "ORDERNAME")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Name);
                        hasCustomOrder = true;


                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Name);
                        hasCustomOrder = true;

                    }
                }
                else if (filterType == "ORDERDESCRIPTION")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u =>u.Description);
                        hasCustomOrder = true;

                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Description);
                        hasCustomOrder = true;

                    }
                }
                else
                {
                    query = query.Where(r => r.Name!.Contains(searchUpper) || (r.Description ?? "").Contains(searchUpper));

                }
            }

            if (filter?.IsActive is bool isActive)
            {
                query = query.Where(u => u.IsActive == isActive);
            }


            if (!hasCustomOrder)
            {
                query = query
                    .OrderByDescending(u => u.CreatedOn);
                    
            }


            var totalCount = await query.CountAsync();
            var roles = await query
                
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RoleListItemDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Description = r.Description,
                    IsActive=r.IsActive

                   
                })
                .ToListAsync();

            return new PagedResult<RoleListItemDto>
            {
                Items = roles,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                
                

            };
        }

        public async Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(RoleCreateDto model)
        {
            if (await _roleManager.RoleExistsAsync(model.Name))
                return (IdentityResult.Failed(new IdentityError { Description = "Role already exists." }), null);



            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;


            var role = new ApplicationRole
            {
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive,
                CreatedOn = DateTime.UtcNow,
                CreadtedBy = _userId,
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = _userId

            };

           

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return (result, null);


            if (!string.IsNullOrEmpty(model.Permissions))
            {
                // Split the comma-separated string into individual permissions
                var permissions = model.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var permission in permissions)
                {
                    var trimmedPermission = permission.Trim(); 
                                                               
                    if (Permissions.GetAll().Contains(trimmedPermission))
                    {
                        await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", trimmedPermission));
                    }
                }
            }
            Console.WriteLine(role.Name, role.IsActive);
            return (result, result.Succeeded ? role.Id : null);
        }

        public async Task<RoleEditDto?> GetForEditAsync(Guid id)
        {
            var role = await _roleManager.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return null;

            var roleClaims = await _roleManager.GetClaimsAsync(role);

            // Filter only "Permission" claims and join them into a comma-separated string
            var permissions = string.Join(", ", roleClaims
                                              .Where(c => c.Type == "Permission")
                                              .Select(c => c.Value));

            return new RoleEditDto
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                IsActive = role.IsActive,
                Permissions=permissions


            };
        }

        public async Task<IdentityResult> UpdateAsync(RoleEditDto model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id.ToString());
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Role not found."
                });
            }
            var dup = await _roleManager.FindByNameAsync(model.Name.Trim());
            if (dup != null && dup.Id != role.Id)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateRoleName",
                    Description = $"Another role already uses this name: {model.Name}"
                });
            }

            var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Safely parse string to Guid — fallback to Guid.Empty
            var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;
            role.Name = model.Name.Trim();
            role.Description = model.Description;
            role.ModifiedBy = _userId;
            role.IsActive = model.IsActive;
            role.ModifiedOn = DateTime.UtcNow;


            



            var currentClaims = await _roleManager.GetClaimsAsync(role);





            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Add new permissions from comma-separated string
            if (!string.IsNullOrEmpty(model.Permissions))
            {
                //var permissions = model.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
                // Split the string, remove empty entries, trim whitespace, and keep only unique values
                var permissions = model.Permissions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)  
                    .Select(p => p.Trim())                             
                    .Where(p => !string.IsNullOrEmpty(p))             
                    .Distinct(StringComparer.OrdinalIgnoreCase)        
                    .ToList();

                foreach (var permission in permissions)
                {
                    var trimmedPermission = permission.Trim();
                    if (Permissions.GetAll().Contains(trimmedPermission))
                    {
                        await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", trimmedPermission));
                    }
                }
            }

            return await _roleManager.UpdateAsync(role);
        }

        public async Task<IdentityResult> ToggleActive(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "Role not found."
                });
            }

            role.IsActive = !role.IsActive;

            var result = await _roleManager.UpdateAsync(role);

            Console.WriteLine(result);
            return result;
        }
        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            // Prevent deletion if role is assigned to any user
            var hasUsers = await _dbContext.Set<IdentityUserRole<Guid>>()
                                           .AsNoTracking()
                                           .AnyAsync(ur => ur.RoleId == id);
            if (hasUsers)
                return IdentityResult.Failed(new IdentityError { Description = "Cannot delete a role that still has users. Remove users from the role first." });

            // Removing the claim asociated with the Role

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            return await _roleManager.DeleteAsync(role);
        }


        public async Task<RoleDetailsDto?> GetDetailsAsync(Guid id, int pageNumber, int pageSize)
        {
            var role = await _roleManager.Roles.AsNoTracking()
                                 .FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return null;


            var usersQuery =
                         from ur in _dbContext.Set<IdentityUserRole<Guid>>().AsNoTracking() //Left table - User Roles
                         join u in _dbContext.Set<ApplicationUser>().AsNoTracking() //Right table - Users
                         on ur.UserId equals u.Id
                         where ur.RoleId == id
                         select new UserInRoleDto
                         {

                             Id = u.Id,
                             Email = u.Email!,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             IsActive = u.IsActive,
                             PhoneNumber = u.PhoneNumber,

                         };

            var total = await usersQuery.CountAsync();
            // Get current page of users
            var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            // Return role details with users
            return new RoleDetailsDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedOn = role.CreatedOn,
                ModifiedOn = role.ModifiedOn,
                Users = new PagedResult<UserInRoleDto>
                {
                    Items = users,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }


    }
}
