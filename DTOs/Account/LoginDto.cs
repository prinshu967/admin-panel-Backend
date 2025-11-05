using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.Account
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email Id is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = "Password Id is Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        
    }
}
