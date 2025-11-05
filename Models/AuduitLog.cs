using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        
        public string UserName { get; set; } = null!;

        
        public string Type { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required.")]
        public string Activity { get; set; } = null!;


     

        // Audit Coolumns
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
       
    }
}
