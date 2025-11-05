using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.ApplicationConfigration;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.ConfigService
{
    public interface IConfigService
    {
        Task<PagedResult<ConfigListItemDto>> GetConfigsAsync(ConfigListFilterDto filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(ConfigCreateDto model);
        Task<ConfigEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(ConfigEditDto model);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
    }
}
