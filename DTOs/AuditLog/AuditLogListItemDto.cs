using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.AuditLog
{
    public class AuditLogListItemDto
    {
        public Guid Id { get; set; }


        public string UserName { get; set; } 


        public string Type { get; set; } 

       
        public string Activity { get; set; } 




        // Audit Coolumns 
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
