using AngularAdminPannel.DTOs.Users;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.DashboardService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAuditLogService _auditLogService;
        public DashboardController(IDashboardService dashboardService ,IAuditLogService auditLogService)
        {
            _dashboardService = dashboardService;
            _auditLogService = auditLogService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _dashboardService.GetAsync();
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";

                await _auditLogService.LogAsync(name, "View", "View Dashboard");
                return Ok(result);

            }
            catch (Exception ) {
                return StatusCode(500, new { Error = "We couldn’t load the Dasboard Details right now. Please try again." });
            }

        }
       
    }
}
