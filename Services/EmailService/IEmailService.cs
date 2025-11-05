namespace AngularAdminPannel.Services.EmailService
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink);

    }
}
