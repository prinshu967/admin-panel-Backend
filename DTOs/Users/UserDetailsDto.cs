namespace AngularAdminPannel.DTOs.Users
{
    public class UserDetailsDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;
        public string? LastName { get; init; }
        public string? PhoneNumber { get; init; }


        public bool IsActive { get; init; }
        public string? ImagePath {get;set;}
        


        public List<string> Roles { get; init; } = new();
    }
}
