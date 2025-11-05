namespace AngularAdminPannel.DTOs.CMS
{
    public class CMSCreateDto

    {
       
        public string Key { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string MetaKeyword { get; set; } = null!;
        public string MetaTitle { get; set; } = null!;
        public string MetaDescription { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        
        
    }
}
