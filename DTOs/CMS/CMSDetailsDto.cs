namespace AngularAdminPannel.DTOs.CMS
{
    public class CMSDetailsDto
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = null!;
        public string Title { get; init; } = null!;
        public string MetaKeyword { get; init; } = null!;
        public string MetaTitle { get; init; } = null!;
        public string MetaDescription { get; init; } = null!;
        public string Content { get; init; } = null!;
        public bool IsActive { get; init; } = true;

    }
}
