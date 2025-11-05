namespace AngularAdminPannel.DTOs.Dashboard
{
    
    public class DashboardResponseDto
    {
        public int TotalUsers { get; set; } = 0;
        public int TotalRoles { get; set; } = 0;

        public List<DashboardInsightDto> DashboardInsights { get; set; } = [];

       
    }
}
