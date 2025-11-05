namespace AngularAdminPannel.Models
{
    public class CMS
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;        
        public string Title { get; set; } = null!;
        public string MetaKeyword { get; set; } = null!;
        public string MetaTitle { get; set; } = null!;
        public string MetaDescription { get; set; } = null!;
        public string Content { get; set; } = null!;   
        public bool IsActive { get; set; } = true;

        // Audit Coolumns
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
    }
}
