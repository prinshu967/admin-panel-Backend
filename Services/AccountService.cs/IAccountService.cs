using AngularAdminPannel.DTOs.Account;
using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Services.AccountService.cs
{
    public interface IAccountService
    {
        Task<LoginResponseDto?> LoginUserAsync(LoginDto model);
        Task LogoutUserAsync();

        Task<bool> SendPasswordResetLinkAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto model);
    }
}
