using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.ApplicationConfigration;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.ConfigService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AngularAdminPannel.Models.Permissions;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationConfigrationController : ControllerBase
    {

        private readonly IConfigService _service;
        private readonly IAuditLogService _auditLogService;
        public ApplicationConfigrationController(IConfigService service , IAuditLogService auditLogService) { 
            _service = service;
            _auditLogService=auditLogService;
                
        
        }


        //GET: api/CMS
        [HttpGet]
        [Authorize(Policy = Configration.View)]
        public async Task<IActionResult> GetConfig([FromQuery] ConfigListFilterDto filterDto)
        {
            try
            {
                var result = await _service.GetConfigsAsync(filterDto);
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";

                await _auditLogService.LogAsync(name, "View", "View Application Configration List");
                return Ok(result);

            }
            catch
            {
                return StatusCode(500, "An unexpected error occurred while retrieving CMS records.");
            }

        }

        // GET: api/CMS/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = Configration.View)]
        public async Task<IActionResult> GetConfig(Guid id)
        {
            try
            {
                var role = await _service.GetForEditAsync(id);
                if (role == null) return NotFound(new { Message = "Configration not found." });

                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";

                await _auditLogService.LogAsync(name, "View", "View Application Configration");
                return Ok(role);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/CMS
        [HttpPost]
        [Authorize(Policy = Configration.Create)]
        public async Task<IActionResult> CreateConfig([FromBody] ConfigCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var (result, cmsId) = await _service.CreateAsync(model);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";


                    await _auditLogService.LogAsync(name, "Create", "Create Application Configration");
                    return Ok(new { Success = true, cmsID = cmsId });
                }

                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/CMS/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = Configration.Edit)]
        public async Task<IActionResult> UpdateConfig(Guid id, [FromBody] ConfigEditDto model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (id != model.Id) return BadRequest(new { Message = "Configration ID mismatch." });

                var result = await _service.UpdateAsync(model);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";


                    await _auditLogService.LogAsync(name, "Update", "Update Application Configration");
                    return Ok(new { Success = true });
                }

                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/CMS/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = Configration.Delete)]
        public async Task<IActionResult> DeleteConfig(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result.Succeeded)
                {

                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";

                    await _auditLogService.LogAsync(name, "Delete", "Delete Application Configration");
                    return Ok(new { Success = true });
                }

                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpPatch("{id:guid}")]
        [Authorize(Policy = CMS.Edit)]
        public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveRequestDto request)
        {
            try
            {
                var result = await _service.ToggleActive(id);

                if (result.Succeeded)
                {
                    // Optional: log the action
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "ToggleActive", $"Toggled Config status To {(request.IsActive ? "Active" : "Inactive")}");

                    return NoContent();
                }

                // Handle NotFound
                if (result.Errors.Any(e => e.Code.Trim() == "NotFound"))
                {
                    return NotFound(result.Errors.Select(e => e.Description));
                }


                return BadRequest(result.Errors.Select(e => e.Description));
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return StatusCode(500, new { Error = "An unexpected error occurred while toggling Config active status." });
            }
        }

    }
}
