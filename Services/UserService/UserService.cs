using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Users;
using AngularAdminPannel.Models;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace AngularAdminPannel.Services.UserService
{
    public class UserService: IUserService
    {
        private const int MaxPageSize = 100;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ClaimsPrincipal? _user;
        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext dbContext,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _user = httpContextAccessor.HttpContext?.User;
        }
        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListFilterDto filter)
        {

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > MaxPageSize ? MaxPageSize : filter.PageSize);
            bool hasCustomOrder = false;

            var query = _userManager.Users.AsNoTracking().AsSplitQuery();
            Console.WriteLine(filter);

            
            if (!string.IsNullOrWhiteSpace(filter.Filter) && !string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                var searchUpper = search.ToUpperInvariant();
                var filterType = filter.Filter.Trim().ToUpperInvariant();


                if (filterType == "EMAIL")
                {
                    query = query.Where(u => u.NormalizedEmail!.Contains(searchUpper));
                }
                else if (filterType=="NAME")
                {
                    query = query.Where(u =>u.FirstName!.Contains(searchUpper));
    


                }
                else if (filterType == "ORDEREMAIL")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.Email);
                        hasCustomOrder = true;
                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.Email);
                        hasCustomOrder = true;
                    }
                }
                else if (filterType == "ORDERNAME")
                {
                    if (searchUpper == "ASC")
                    {
                        //query = query.OrderBy(u => (u.FirstName ?? "") + " " + (u.LastName ?? ""));
                        query = query.OrderBy(u => u.FirstName);
             



                        hasCustomOrder = true;
                    }
                    else
                    {
                        //query = query.OrderByDescending(u => (u.FirstName ?? "") + " " + (u.LastName ?? ""));
                        query = query.OrderByDescending(u => u.FirstName);

                        hasCustomOrder = true;
                    }
                }
                else if (filterType == "ORDERPHONE")
                {
                    if (searchUpper == "ASC")
                    {
                        query = query.OrderBy(u => u.PhoneNumber);
                        hasCustomOrder = true;
                    }
                    else
                    {
                        query = query.OrderByDescending(u => u.PhoneNumber);
                        hasCustomOrder = true;
                    }
                }
                else if (search.All(char.IsDigit) && filterType == "PHONE")
                {
                    query = query.Where(u => (u.PhoneNumber ?? "").Contains(search));
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
            var total = await query.CountAsync();

            var items = await query
                 
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)
                 .Select(u => new UserListItemDto
                 {
                     Id = u.Id,
                     Email = u.Email!,

                     FirstName = u.FirstName,
                     LastName = u.LastName,
                     PhoneNumber = u.PhoneNumber,
                     IsActive = u.IsActive,
                     CreatedOn = u.CreatedOn,
                     ImagePath = u.ImagePath == null ? "" : $"https://localhost:7001/{u.ImagePath}"

                 })
                 .ToListAsync();

            return new PagedResult<UserListItemDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


        public async Task<(IdentityResult Result, Guid? UserId)> CreateAsync(UserCreateDto model)
        {
            // ExecutionStrategy adds resiliency (automatic retries for transient SQL errors)
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<(IdentityResult, Guid?)>(async () =>
            {
                // Start an explicit transaction
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {

                    string? imagePath = null;

                    if(model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var webRoot = _webHostEnvironment.WebRootPath;
                        if (string.IsNullOrEmpty(webRoot))
                        {
                            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        }

                        var uploadsFolder = Path.Combine(webRoot, "uploads", "users");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file using IFormFile API
                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        // Store relative path for use in URLs
                        imagePath = Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
                        Console.WriteLine(imagePath);

                    }

                    
                    var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    // Safely parse string to Guid — fallback to Guid.Empty
                    var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName?.Trim(),
                        Email = model.Email.Trim(),
                        UserName = model.Email.Trim(),
                        PhoneNumber = model.PhoneNumber,

                        IsActive = model.IsActive,

                        CreatedOn = DateTime.UtcNow,
                        CreadtedBy =_userId,
                        ModifiedOn = DateTime.UtcNow,
                        ModifiedBy = _userId,
                        ImagePath=imagePath
                    };

                    //var create = await _userManager.CreateAsync(user, model.Password); // Create password latter when 
                    IdentityResult create;
                    if (!string.IsNullOrWhiteSpace(model.Password))
                        create = await _userManager.CreateAsync(user, model.Password);
                    else
                        create = await _userManager.CreateAsync(user);

                    if (!create.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return (create, null);
                    }
                    var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                    if (!roleExists)
                    {
                        await transaction.RollbackAsync();
                        return (IdentityResult.Failed(new IdentityError
                        {
                            Code = "RoleNotFound",
                            Description = $"Role '{model.RoleName}' does not exist."
                        }), null);
                    }

                    // Assign role
                    var roleResult = await _userManager.AddToRoleAsync(user, model.RoleName);
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return (roleResult, null);
                    }
                    await transaction.CommitAsync();
                    return (IdentityResult.Success, user.Id);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    await transaction.RollbackAsync();
                    throw; 
                }
            });
        }


        public async Task<UserEditDto?> GetForEditAsync(Guid id)
        {
            // AsNoTracking -> we don't need change tracking for display
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return null;
           
            return new UserEditDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ImagePath = user.ImagePath == null ? "" : $"https://localhost:7001/{user.ImagePath}",

                IsActive = user.IsActive,


            };
        }
        public async Task<IdentityResult> UpdateProfile(Guid id, ProfileUpdateDto model)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Ensure user can only update their own profile
                    if (id != model.Id)
                    {
                        return IdentityResult.Failed(new IdentityError { Code = "Unauthorized", Description = "You are not authorized to update this profile." });
                    }

                    var user = await _userManager.FindByIdAsync(model.Id.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }

                    // Handle email change (optional)
                    if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
                        if (!emailResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return emailResult;
                        }

                        var usernameResult = await _userManager.SetUserNameAsync(user, model.Email.Trim());
                        if (!usernameResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return usernameResult;
                        }
                    }

                    // Handle password change (requires current password verification)
                    if (!string.IsNullOrWhiteSpace(model.CurrentPassword) &&
                        !string.IsNullOrWhiteSpace(model.NewPassword) &&
                        !string.IsNullOrWhiteSpace(model.ConfirmPassword))
                    {
                        if (model.NewPassword != model.ConfirmPassword)
                        {
                            await transaction.RollbackAsync();
                            return IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "New password and confirmation do not match." });
                        }

                        var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                        if (!passwordCheck)
                        {
                            await transaction.RollbackAsync();
                            return IdentityResult.Failed(new IdentityError { Code = "InvalidPassword", Description = "Current password is incorrect." });
                        }

                        var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                        if (!passwordChangeResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return passwordChangeResult;
                        }
                    }

                    // Handle image upload
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadsFolder = Path.Combine(webRoot, "uploads", "users");

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        var imagePath = Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
                        user.ImagePath = imagePath;
                    }

                    // Update profile details
                    user.FirstName = model.FirstName?.Trim();
                    user.LastName = model.LastName?.Trim();
                    user.PhoneNumber = model.PhoneNumber?.Trim();
                    user.ModifiedOn = DateTime.UtcNow;
                    user.ModifiedBy = id; // Self-update

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return updateResult;
                    }

                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        public async Task<IdentityResult> UpdateAsync(UserEditDto model)
        {
            // ExecutionStrategy adds resiliency (automatic retries for transient SQL errors)
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(model.Id.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }


                    // If email changed, update both Email & UserName (Identity will SaveChanges inside the transaction)
                    if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
                        if (!emailResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return emailResult;
                        }
                        var usernameResult = await _userManager.SetUserNameAsync(user, model.Email.Trim());
                        if (!usernameResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return usernameResult;
                        }
                    }
                    // Handle role updates (only one role per user)
                    if (!string.IsNullOrEmpty(model.RoleName))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);

                        // Check if role needs to be changed
                        if (!currentRoles.Contains(model.RoleName))
                        {
                            // Remove all current roles (since only one is allowed)
                            if (currentRoles.Any())
                            {
                                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                                if (!removeResult.Succeeded)
                                {
                                    await transaction.RollbackAsync();
                                    return removeResult;
                                }
                            }

                            // Add the new role
                            var addResult = await _userManager.AddToRoleAsync(user, model.RoleName);
                            if (!addResult.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                return addResult;
                            }
                        }
                    }

                    string? imagePath = null;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var webRoot = _webHostEnvironment.WebRootPath;
                        if (string.IsNullOrEmpty(webRoot))
                        {
                            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        }

                        var uploadsFolder = Path.Combine(webRoot, "uploads", "users");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file using IFormFile API
                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        // Store relative path for use in URLs
                        imagePath = Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
                        Console.WriteLine(imagePath);
                        user.ImagePath = imagePath;

                    }
                    var idString = _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    // Safely parse string to Guid — fallback to Guid.Empty
                    var _userId = Guid.TryParse(idString, out var parsedGuid) ? parsedGuid : Guid.Empty;

                    // Update profile fields
                    user.FirstName = model.FirstName.Trim();
                    user.LastName = model.LastName?.Trim();
                    user.PhoneNumber = model.PhoneNumber;


                    user.IsActive = model.IsActive;

                    user.ModifiedOn = DateTime.UtcNow;
                    user.ModifiedBy = _userId;


                    // --- PASSWORD CHANGE (NO OLD PASSWORD) ---
                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        // Remove existing password if exists
                        var removePassResult = await _userManager.RemovePasswordAsync(user);
                        if (!removePassResult.Succeeded)
                        {
                            // Ignore "User does not have a password" error
                            if (!removePassResult.Errors.Any(e => e.Code == "UserPasswordNotSet"))
                            {
                                await transaction.RollbackAsync();
                                return removePassResult;
                            }
                        }

                        // Add the new password
                        var addPassResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        if (!addPassResult.Succeeded)
                        {
                            await transaction.RollbackAsync();
                            return addPassResult;
                        }

                        // Optional: Invalidate all current sessions for security
                        await _userManager.UpdateSecurityStampAsync(user);
                    }


                    var update = await _userManager.UpdateAsync(user);
                    if (!update.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return update;
                    }
                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }



        public async Task<UserDetailsDto?> GetDetailsAsync(Guid id)
        {
            // Read-only entity for display
            var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return null;
            // Identity API requires the user entity for role lookup
            // Returns the Role in string format
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDetailsDto
            {
                Id = user.Id,
                Email = user.Email!,

                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,

                IsActive = user.IsActive,
                ImagePath= user.ImagePath == null ? "" : $"https://localhost:7001/{user.ImagePath}",

                Roles = roles.OrderBy(r => r).ToList()
            };
        }
       

        public async Task<IdentityResult> ToggleActive(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "NotFound",
                    Description = "User not found."
                });
            }

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);

            Console.WriteLine(result);
            return result;
        }
        // Deletes a user with a guard to prevent removing the last Admin.

        public async Task<IdentityResult> DeleteAsync(Guid id)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IdentityResult>(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                    {
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });
                    }
                    // Safety: block deleting the last "Admin"
                    var adminRole = await _roleManager.FindByNameAsync("Admin");
                    if (adminRole != null)
                    {
                        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (isAdmin)
                        {
                            var anotherAdminExists = await _dbContext.Set<IdentityUserRole<Guid>>()
                            .AnyAsync(ur => ur.RoleId == adminRole.Id && ur.UserId != user.Id);
                            if (!anotherAdminExists)
                            {
                                await transaction.RollbackAsync();
                                return IdentityResult.Failed(new IdentityError
                                {
                                    Code = "LastAdmin",
                                    Description = "You cannot delete the last user in the 'Admin' role."
                                });
                            }
                        }
                    }
                    var delete = await _userManager.DeleteAsync(user);
                    if (!delete.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return delete;
                    }
                    await transaction.CommitAsync();
                    return IdentityResult.Success;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }


        // No use below



        public async Task<UserRolesEditDto?> GetRolesForEditAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return null;
            // List all active roles (read-only)
            var allRoles = await _roleManager.Roles
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedOn)
            .Where(r => r.IsActive)
            .ToListAsync();
            // Current assignments for the user
            var assignedRoles = await _userManager.GetRolesAsync(user);
            // Case-insensitive check to avoid surprises with different normalizations
            var userRolesEditDto = new UserRolesEditDto
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Roles = allRoles.Select(role => new RoleCheckboxDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    Description = role.Description,
                    IsSelected = assignedRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase)
                }).ToList()
            };
            return userRolesEditDto;
        }
        // Updates a user's roles using batched operations
        public async Task<IdentityResult> UpdateRolesAsync(Guid userId, IEnumerable<Guid> selectedRoleIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Code = "NotFound", Description = "User not found." });

            var ids = (selectedRoleIds ?? Enumerable.Empty<Guid>()).Distinct().ToList();

            var selectedRoleNames = ids.Count == 0
                ? new List<string>()
                : await _roleManager.Roles
                    .AsNoTracking()
                    .Where(r => ids.Contains(r.Id))
                    .Select(r => r.Name!)
                    .ToListAsync();

            if (selectedRoleNames.Count != ids.Count)
                return IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = "One or more selected roles do not exist." });

            var currentRoles = await _userManager.GetRolesAsync(user);

            var currentSet = new HashSet<string>(currentRoles, StringComparer.OrdinalIgnoreCase);
            var targetSet = new HashSet<string>(selectedRoleNames, StringComparer.OrdinalIgnoreCase);

            var toAdd = targetSet.Except(currentSet).ToList();
            var toRemove = currentSet.Except(targetSet).ToList();

            if (!toAdd.Any() && !toRemove.Any())
                return IdentityResult.Success;

            IdentityResult? result = null;
            if (toAdd.Any())
            {
                result = await _userManager.AddToRolesAsync(user, toAdd);
                if (!result.Succeeded) return result;
            }
            if (toRemove.Any())
            {
                result = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!result.Succeeded) return result;
            }

            return IdentityResult.Success;
        }



    }
}
