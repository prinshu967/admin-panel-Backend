using AngularAdminPannel.DTOs;
using AngularAdminPannel.DTOs.Users;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static AngularAdminPannel.Models.Permissions;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {


        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;

        public UsersController(IUserService userService, IAuditLogService auditLogService)
        {
            _userService = userService;
            _auditLogService = auditLogService;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Policy = Users.View)]

        public async Task<IActionResult> Get([FromQuery] UserListFilterDto filter)
        {

            Console.WriteLine(filter.Filter);
            try


            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _userService.GetUsersAsync(filter);
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";
                await _auditLogService.LogAsync(name, "View", "View User List");
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "We couldn’t load the users right now. Please try again." });

            }
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Policy = Users.Create)]
        public async Task<IActionResult> Create([FromForm] UserCreateDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (result, newId) = await _userService.CreateAsync(model);
                if (result.Succeeded)

                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Create", "Create User");
                    return CreatedAtAction(nameof(GetById), new { id = newId }, model);

                }

                return BadRequest(result.Errors.Select(e => e.Description));
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred while creating the user." });
            }
        }

        // GET: api/Users/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Policy = Users.View)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var user = await _userService.GetDetailsAsync(id);
                if (user == null)
                    return NotFound(new { Error = "User not found." });
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";

                await _auditLogService.LogAsync(name, "View", "View User");

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "We couldn’t load the user details. Please try again." });
            }
        }

        // PUT: api/Users/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Policy = Users.Edit)]
        public async Task<IActionResult> Update(Guid id, [FromForm] UserEditDto model)
        {
            if (id != model.Id)
                return BadRequest(new { Error = "User ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userService.UpdateAsync(model);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Update", "Update User ");
                    return NoContent();
                }

                if (result.Errors.Any(e => string.Equals(e.Code, "ConcurrencyFailure", StringComparison.OrdinalIgnoreCase)))
                    return Conflict(new { Error = "This user was modified by another admin. Please reload and try again." });

                return BadRequest(result.Errors.Select(e => e.Description));
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred while updating the user." });
            }
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Users.Delete)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { Error = "Invalid user ID." });

            try
            {
                var result = await _userService.DeleteAsync(id);
                if (result.Succeeded)
                {
                    var firstName = User.FindFirst("FirstName")?.Value;
                    var lastName = User.FindFirst("LastName")?.Value;

                    // Combine first and last name, and fallback if empty
                    var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                               ? "Anonymous User"
                               : $"{firstName} {lastName}";
                    await _auditLogService.LogAsync(name, "Delete", "Delete User");
                    return NoContent();
                }

                if (result.Errors.Any(e => string.Equals(e.Code, "LastAdmin", StringComparison.OrdinalIgnoreCase)))
                    return BadRequest(new { Error = "You cannot delete the last user in the ‘Admin’ role." });

                if (result.Errors.Any(e => string.Equals(e.Code, "NotFound", StringComparison.OrdinalIgnoreCase)))
                    return NotFound(new { Error = "The user no longer exists." });

                return BadRequest(result.Errors.Select(e => e.Description));
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred while deleting the user." });
            }
        }

        [HttpGet("GetProfile")]

        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized("Invalid user token.");

                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid user ID format in token.");

                var user = await _userService.GetDetailsAsync(userId);
                if (user == null)
                    return NotFound(new { Error = "User not found." });
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";

                await _auditLogService.LogAsync(name, "View", "View Profile");

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "We couldn’t load the user details. Please try again." });
            }
        }


        [HttpPut("UpdateProfile")]

        public async Task<IActionResult> UpdateProfile([FromForm] ProfileUpdateDto model)
        {

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid user token.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid user ID format in token.");


            var result = await _userService.UpdateProfile(userId, model);


            if (result.Succeeded)
            {
                var firstName = User.FindFirst("FirstName")?.Value;
                var lastName = User.FindFirst("LastName")?.Value;

                // Combine first and last name, and fallback if empty
                var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                           ? "Anonymous User"
                           : $"{firstName} {lastName}";
                await _auditLogService.LogAsync(name, "Update", "Update Profile ");
                return Ok(new { message = "Profile updated successfully." });
            }

            return BadRequest(new
            {
                message = "Profile update failed.",
               

                errors = result.Errors
            });
        }


        // Toggle Activity

        [HttpPatch("{id:guid}")]
        [Authorize(Policy = Users.Edit)]
        public async Task<IActionResult> ToggleActive(Guid id, [FromBody] ToggleActiveRequestDto request)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID." });
            }
            var result = await _userService.ToggleActive(id);

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

            await _auditLogService.LogAsync(name, "ToggleActive", $"Toggled User status To {(request.IsActive ? "Active" : "Inactive")}");

            return Ok(new { message = "User active status toggled successfully." });
        }
    





        // GET: api/Users/{id}/Roles
        [HttpGet("{id:guid}/roles")]
        public async Task<IActionResult> GetRoles(Guid id)
        {
            try
            {
                var roles = await _userService.GetRolesForEditAsync(id);
                if (roles == null)
                    return NotFound(new { Error = "User not found." });

                return Ok(roles);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "We couldn’t load the roles. Please try again." });
            }
        }

        // PUT: api/Users/{id}/Roles
        [HttpPut("{id:guid}/roles")]
        public async Task<IActionResult> UpdateRoles(Guid id, UserRolesEditDto model)
        {
            if (model == null || model.UserId != id)
                return BadRequest(new { Error = "Invalid request." });

            try
            {
                var selected = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleId).ToList();
                var result = await _userService.UpdateRolesAsync(id, selected);
                if (result.Succeeded)
                    return NoContent();

                if (result.Errors.Any(e => string.Equals(e.Code, "RoleNotFound", StringComparison.OrdinalIgnoreCase)))
                    return BadRequest(new { Error = "One or more selected roles no longer exist. Please refresh and try again." });

                return BadRequest(result.Errors.Select(e => e.Description));
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred while updating roles." });
            }
        }

    }
}
