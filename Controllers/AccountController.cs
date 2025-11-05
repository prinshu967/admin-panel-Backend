using AngularAdminPannel.DTOs.Account;
using AngularAdminPannel.Services.AccountService.cs;
using AngularAdminPannel.Services.AuditLogService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularAdminPannel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuditLogService _auditLogService;

        public AccountController(IAccountService accountService, IAuditLogService auditLogService)
        {
            _accountService = accountService;
            _auditLogService = auditLogService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var response = await _accountService.LoginUserAsync(model);

            if (response == null)
                return Unauthorized(new { message = "Invalid email or password" });

         
            
            

            
            return Ok(new
            {
                message = "Login successful",
                data = response
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutUserAsync();
            await _auditLogService.LogAsync("Prinshu Kumar", "Logout", "Logout ");
            return Ok(new { message = "Logged out successfully" });
        }


        /// <summary>
        /// Sends a password reset link to the user's email
        /// </summary>
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountService.SendPasswordResetLinkAsync(model.Email);

            // Always return success message to avoid leaking user existence
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;

            // Combine first and last name, and fallback if empty
            var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                       ? "Anonymous User"
                       : $"{firstName} {lastName}";

            await _auditLogService.LogAsync(name, "Foget Password", "Forgot password page ");
            return Ok(new
            {
                Message = "If an account with this email exists, a password reset link has been sent."
            });
        }

        
        // Resets the user's password using the token from email
        
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _accountService.ResetPasswordAsync(model);

            if (!result.Succeeded)
            {
                // Collect all errors
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Password reset failed.", Errors = errors });
            }
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;

            // Combine first and last name, and fallback if empty
            var name = string.IsNullOrWhiteSpace($"{firstName} {lastName}")
                       ? "Anonymous User"
                       : $"{firstName} {lastName}";

            await _auditLogService.LogAsync(name, "Reset password", "Reset Password");
            return Ok(new { Message = "Password has been reset successfully." });
        }
    }
}
