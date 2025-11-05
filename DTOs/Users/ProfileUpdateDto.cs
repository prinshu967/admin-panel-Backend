using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AngularAdminPannel.DTOs.Users
{
    public class ProfileUpdateDto
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
        [MaxLength(100,ErrorMessage ="Email is Maximmum 100 charater Long")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? PhoneNumber { get; set; }

        // Optional — included if you want to allow self-role change (often restricted by admin)
        public string? RoleName { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ImagePath { get; set; }

        public IFormFile? ImageFile { get; set; }

        
        // Password fields for secure profile updates
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
