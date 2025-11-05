using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.Models
{
    public class ApplicationConfig
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Key is required.")]
        public string Key { get; set; } = null!;

        [Required(ErrorMessage = "Value is required.")]
        public string Value { get; set; } = null!;


       
        public bool IsActive { get; set; } = true;


        public int DisplayOrder { get; set; } = 0;

      

        // Audit Columns
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
