using AngularAdminPanel.DTOs.EmailTemplate;
using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.EmailTemplate;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.EmailTemplateService
{
    public interface IEmailTemplateService
    {
        Task<PagedResult<EmailTemplateListItemDto>> GetEmailTemplatesAsync(EmailTemplateListFilterDto filter);
        Task<(IdentityResult Result, Guid? RoleId)> CreateAsync(EmailTemplateCreateDto model);
        Task<EmailTemplateEditDto?> GetForEditAsync(Guid id);
        Task<IdentityResult> UpdateAsync(EmailTemplateEditDto model);
        Task<IdentityResult> ToggleActive(Guid id);
        Task<IdentityResult> DeleteAsync(Guid id);
    }
}
  