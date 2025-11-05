using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.AuditLog;

namespace AngularAdminPannel.Services.AuditLogService
{
    public interface IAuditLogService
    {
        Task<PagedResult<AuditLogListItemDto>> GetAuditAsync(AuditLogFilterDto filterDto);
        Task LogAsync(string userName, string type, string activity);

    }
}
