using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Roles;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.RoleService
{
    public interface IRoleService
    {
        Task<PagedResult<RoleListItemDto>> GetRolesAsync(RoleListFilterDto filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(RoleCreateDto model);
        Task<RoleEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(RoleEditDto model);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
        Task<RoleDetailsDto?> GetDetailsAsync(Guid id, int pageNumber, int pageSize);
    }
}
