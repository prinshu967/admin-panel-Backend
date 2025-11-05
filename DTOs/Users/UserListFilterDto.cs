namespace AngularAdminPannel.DTOs.Users
{
    public class UserListFilterDto
    {
        // Search text
        public string? Search { get; set; }

        // Filter type (e.g., EMAIL)
        public string? Filter { get; set; }

        

        // Active status
        public bool? IsActive { get; set; }

        // Default page number
        public int PageNumber { get; set; } = 1;

        // Default page size
        public int PageSize { get; set; } = 5;
    }
}
