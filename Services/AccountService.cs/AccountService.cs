using AngularAdminPannel.Data;
using AngularAdminPannel.DTOs.Account;
using AngularAdminPannel.Models;
using AngularAdminPannel.Services.AuditLogService;
using AngularAdminPannel.Services.EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AngularAdminPannel.Services.AccountService.cs
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAuditLogService _auditLogService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config, IEmailService emailService,
            RoleManager<ApplicationRole> roleManager,
            IAuditLogService auditLogService
            
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _emailService = emailService;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
        }

        public async Task<LoginResponseDto?> LoginUserAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return null;

            // Generate JWT token
            var jwtSettings = _config.GetSection("JwtSettings");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //foreach (var role in roles)
            //    claims.Add(new Claim(ClaimTypes.Role, role));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                // Add permission claims from the role
                var appRole = await _roleManager.FindByNameAsync(role);
                if (appRole != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(appRole);
                    foreach (var claim in roleClaims.Where(c => c.Type == "Permission"))
                    {
                        claims.Add(claim);
                    }
                }
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["AccessTokenExpiryMinutes"] ?? "60")),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            await _auditLogService.LogAsync($"{user.FirstName} {user.LastName}", "Login", "Login");


            return new LoginResponseDto
            {
                Token = tokenString,
                ExpiresAt = token.ValidTo,
                UserId = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                UserName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToList(),
                ImagePath= user.ImagePath == null ? "" : $"https://localhost:7001/{user.ImagePath}"
            };
        }

        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }





        public async Task<bool> SendPasswordResetLinkAsync(string email)
        {
            // Try to find the user by their email address
            var sw = new Stopwatch();
            sw.Start();
            var user = await _userManager.FindByEmailAsync(email);
            sw.Stop();
            Console.WriteLine($"Email check : {sw.ElapsedMilliseconds.ToString()}");

            // Security measure: 
            // Do not reveal whether the user exists or not — 
            // always behave the same if the user is not found or the email is not confirmed
            if (user == null)
                return false;

            // Generate a unique, secure token for password reset
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token so it can be safely used in a URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Construct the password reset link with the encoded token and user’s email
            var baseUrl = _config["AppSettings:BaseUrl"];
            var resetLink = $"{baseUrl}/?email={user.Email}&token={encodedToken}";
            Console.WriteLine(resetLink);

            // Send the reset link via email to the user
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FirstName, resetLink);

            return true;
        }


        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model)
        {
            // Find the user associated with the provided email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // If user not found, return a generic failure (no details leaked for security)
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });

            // Decode the token that was passed in from the reset link
            var decodedBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            // Attempt to reset the user's password with the new one
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

            // If successful, update the Security Stamp to invalidate any active sessions or tokens
            if (result.Succeeded)
                await _userManager.UpdateSecurityStampAsync(user);

            return result;
        }
    }
}
