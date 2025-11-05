using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.FAQ
{
    public class FAQCreateDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Question is required.")]
        public string Question { get; set; } = null!;

        [Required(ErrorMessage = "Please select the status (Active/Inactive).")]
        public bool IsActive { get; set; } = true;


        public int DisplayOrder { get; set; } = 0;

        public string Answer { get; set; } = null!;
    }
}
