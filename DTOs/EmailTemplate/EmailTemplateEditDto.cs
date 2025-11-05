using System;
using System.ComponentModel.DataAnnotations;

namespace AngularAdminPanel.DTOs.EmailTemplate
{
    public class EmailTemplateEditDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter a unique key for the template.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the title of the email template.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "From Name is required.")]
        public string FromName { get; set; } = string.Empty;

        [Required(ErrorMessage = "From Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string FromEmail { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsManualMail { get; set; }

        public bool IsContactUsMail { get; set; }

        [Required(ErrorMessage = "Email body content is required.")]
        public string Body { get; set; } = string.Empty;
    }
}
