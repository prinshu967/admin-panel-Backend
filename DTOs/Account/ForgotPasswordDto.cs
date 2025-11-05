using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.Account
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Please enter your Email address.")]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        public string Email { get; set; } = null!;
    }
}
