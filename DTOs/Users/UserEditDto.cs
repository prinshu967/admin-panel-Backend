using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.Users
{
    public class UserEditDto
    {
        [Required]
        public Guid Id { get; init; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [RegularExpression("^[a-zA-Z ]+$", ErrorMessage = "First name can only contain letters and spaces.")]
        public string FirstName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public string? ImagePath { get; set; }
        public string? NewPassword { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
