using Microsoft.AspNetCore.Identity;

namespace AngularAdminPannel.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {

        
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        

        // Audit Columns
        public DateTime? CreatedOn { get; set; }
        public Guid? CreadtedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid ModifiedBy { get; set; }

        public ICollection<IdentityRoleClaim<string>>RoleClaims { get; set; }
    }
}
