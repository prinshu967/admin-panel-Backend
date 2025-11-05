using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Users;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.UserService
{
    public interface IUserService
    {
        Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListFilterDto filter);
        Task<(IdentityResult Result, Guid? UserId)> CreateAsync(UserCreateDto model);
        Task<UserEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(UserEditDto model);
        Task<UserDetailsDto?> GetDetailsAsync(Guid id);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
        Task<IdentityResult> UpdateProfile(Guid id, ProfileUpdateDto model);

        // Must chage the Functionlities of last two method


        Task<UserRolesEditDto?> GetRolesForEditAsync(Guid userId);
        Task<IdentityResult> UpdateRolesAsync(Guid userId, IEnumerable<Guid> selectedRoleIds);
    }
}
