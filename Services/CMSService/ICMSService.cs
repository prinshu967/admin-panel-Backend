using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.CMS;
using AngularAdminPannel.DTOs.Roles;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.CMSService
{
    public interface ICMSService
    {
        Task<PagedResult<CMSListItemDto>> GetCMSsAsync(CMSListFilterDto filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(CMSCreateDto model);
        Task<CMSEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(CMSEditDto model);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
        //        Task<CMSDetailsDto?> GetDetailsAsync(Guid id, int pageNumber, int pageSize);
    }
}
