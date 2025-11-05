using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.EmailTemplate
{
    public class EmailTemplateListFilterDto
    {
        [StringLength(50, ErrorMessage = "Search text cannot exceed 50 characters.")]
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
