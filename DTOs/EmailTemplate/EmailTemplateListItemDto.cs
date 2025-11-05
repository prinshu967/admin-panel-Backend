using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.EmailTemplate
{
    public class EmailTemplateListItemDto
    {

        [Required(ErrorMessage ="Id is Required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter a unique key for the template.")]
        public string Key { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the title of the email template.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = null!;

        public bool IsActive { get; set; } = true;


    }
}
