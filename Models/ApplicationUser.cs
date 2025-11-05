using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularAdminPannel.Models
{
    [Index(nameof(FirstName))]
    [Index(nameof(LastName))]
    [Index(nameof(PhoneNumber))]
    [Index(nameof(IsActive))]
    [Index(nameof(Email))]
    
    [Index(nameof(CreatedOn), IsDescending = new[] { true })]
    // Ascending index
    [Index(nameof(CreatedOn), IsDescending = new[] { false })]
    public class ApplicationUser : IdentityUser<Guid>
    {
        // ======================
        // Personal Information
        // ======================

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First name can only contain alphabets and spaces")]
        public string FirstName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Za-z\s]*$", ErrorMessage = "Last name can only contain alphabets and spaces")]
        public string? LastName { get; set; }

        public bool IsActive { get; set; }

        public string? ImagePath { get; set; }

        // ======================
        // Authentication Fields
        // ======================

        [Required(ErrorMessage = "Username is required")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 15 characters")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Username must be alphanumeric with no spaces")]
        public override string UserName
        {
            get => base.UserName!;
            set => base.UserName = value;
        }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format (e.g., name@domain.com)")]
        public override string Email
        {
            get => base.Email!;
            set => base.Email = value;
        }
       
        [NotMapped] // Do not map password directly into DB
        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be numeric (10–15 digits), with optional country code")]
        public override string PhoneNumber
        {
            get => base.PhoneNumber!;
            set => base.PhoneNumber = value;
        }

        // ======================
        // Audit Columns
        // ======================

        public DateTime? CreatedOn { get; set; }

        public Guid? CreadtedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public Guid? ModifiedBy { get; set; }
    }
}
