using System;
using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.Models
{
    public class FAQ
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Question is required.")]
        public string Question { get; set; } = null!;

        [Required(ErrorMessage = "Please select the status (Active/Inactive).")]
        public bool IsActive { get; set; } = true;

       
        public int DisplayOrder { get; set; } = 0;

        public string Answer { get; set; } = null!;

        // Audit Columns
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
