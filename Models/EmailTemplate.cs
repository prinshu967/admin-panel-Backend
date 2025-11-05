using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.Models
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter a unique key for the template.")]
        public string Key { get; set; } = null!;

        [Required(ErrorMessage = "Please enter the title of the email template.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "From Name is required.")]
        public string FromName { get; set; } = null!;

        [Required(ErrorMessage = "From Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string FromEmail { get; set; } = null!;

        [Required(ErrorMessage = "Please select the status (Active/Inactive).")]
        public bool IsActive { get; set; } = true;

        public bool IsManualMail { get; set; }

        public bool IsContactUsMail { get; set; }

        [Required(ErrorMessage = "Email body content is required.")]
        public string Body { get; set; } = null!;

        // Audit Coolumns
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
