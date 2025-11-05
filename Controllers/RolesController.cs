using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Roles;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.RoleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AngularAdminPannel.Models.Permissions;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {

        private readonly IRoleService _roleService;
        private readonly IAuditLogService _auditLogService;

        public RolesController(IRoleService roleService, IAuditLogService auditLogService)
        {
            _roleService = roleService;
            _auditLogService = auditLogService;
        }

        // GET: api/Role/Roles
        [HttpGet("Roles")]
        [Authorize(Policy = Roles.View)]
        public async Task<IActionResult> GetRoles([FromQuery] RoleListFilterDto filter)
        {
            try
            {
                var result = await _roleService.GetRolesAsync(filter);
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";
                await _auditLogService.LogAsync(name, "View", "View Role List");
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/Role/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = Roles.View)]
        public async Task<IActionResult> GetRole(Guid id)
        {
            try
            {
                var role = await _roleService.GetForEditAsync(id);
                if (role == null) return NotFound(new { Message = "Role not found." });
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";
                await _auditLogService.LogAsync(name, "View", "View Role");
                return Ok(role);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }


        // POST: api/Role/Create
        [HttpPost("Create")]
        [Authorize(Policy = Roles.Create)]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var (result, roleId) = await _roleService.CreateAsync(model);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Create", "Create Role");
                    return Ok(new { Success = true, RoleId = roleId });


                }

                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }



        // PUT: api/Role/Update/{id}
        [HttpPut("Update/{id}")]
        [Authorize(Policy = Roles.Edit)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleEditDto model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                if (id != model.Id) return BadRequest(new { Message = "Role ID mismatch." });

                var result = await _roleService.UpdateAsync(model);
                if (result.Succeeded)

                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Update", "Update Role");

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
        [Authorize(Policy = Roles.Edit)]
        public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveRequestDto request)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID." });
            }
            var result = await _roleService.ToggleActive(id);

            if (!result.Succeeded)
            {

                var notFoundError = result.Errors.FirstOrDefault(e => e.Code == "NotFound");
                if (notFoundError != null)
                {
                    return NotFound(new
                    {
                        error = notFoundError.Description
                    });
                }

                // Otherwise, return general Identity errors
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => new { e.Code, e.Description })
                });
            }
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;

            // Combine first and last name, and fallback if empty
            var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                       ? "Anonymous User"
                       : $"{firstName} {lastName}";

            await _auditLogService.LogAsync(name, "ToggleActive", $"Toggled Role status To {(request.IsActive?"Active":"InActive")}");

            return Ok(new { message = "User active status toggled successfully." });
        }


        // DELETE: api/Role/Delete/{id}
        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = Roles.Delete)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Delete", "Delete Role");
                    return Ok(new { Success = true });
                }

                return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/Role/{id}/Users?pageNumber=1&pageSize=10
        [HttpGet("{id}/Users")]
        public async Task<IActionResult> GetUsersInRole(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _roleService.GetDetailsAsync(id, pageNumber, pageSize);
                if (result == null) return NotFound(new { Message = "Role not found or no users assigned." });
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        


    }
}
