using AngularAdminPannel.DTOs.AuditLog;
using AngularAdminPannel.DTOs.CMS;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.CMSService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AngularAdminPannel.Models.Permissions;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;
        private readonly IAuditLogService _auditLogService;
        public AuditLogController(IAuditLogService service, IAuditLogService auditLogService)
        {
            _service = service;
            _auditLogService = auditLogService;

        }

        //GET: api/CMS
        [HttpGet]



        [Authorize(Policy = AuditLog.View)]
        public async Task<IActionResult> GetCMS([FromQuery] AuditLogFilterDto filterDto)
        {
            try
            {
                var result = await _service.GetAuditAsync(filterDto);
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";
                await _auditLogService.LogAsync(name, "View", "View Audit Log List ");
                return Ok(result);

            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred while retrieving Audit Log records.");
            }

        }
    }
} 
