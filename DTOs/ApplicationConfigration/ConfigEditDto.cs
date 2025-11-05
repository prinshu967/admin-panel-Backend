using System.ComponentModel.DataAnnotations;

namespace AngularAdminPannel.DTOs.ApplicationConfigration
{
    public class ConfigEditDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Key is required.")]
        public string Key { get; set; } = null!;

        [Required(ErrorMessage = "Value is required.")]
        public string Value { get; set; } = null!;



        public bool IsActive { get; set; } = true;


        public int DisplayOrder { get; set; } = 0;
    }
}
