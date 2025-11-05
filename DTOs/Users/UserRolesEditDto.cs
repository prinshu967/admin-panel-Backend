using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.Users
{
    public class UserRolesEditDto
    {
        [Required(ErrorMessage = "Invalid user.")]
        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
        // Roles collection can be empty (no roles selected), so no Required here.
        public List<RoleCheckboxDto> Roles { get; set; } = new();
    }
}
