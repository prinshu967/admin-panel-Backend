using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.FAQ;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.FAQService
{
    public interface IFAQService
    {
        Task<PagedResult<FAQListItemDto>> GetFAQsAsync(FAQListFilterDto filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(FAQCreateDto model);
        Task<FAQEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(FAQEditDto model);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
    }
}
