using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.ApplicationConfigration;
using AngularAdminPannel.DTOs.Dashboard;

namespace AngularAdminPannel.Services.DashboardService
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetAsync();
    }
}
