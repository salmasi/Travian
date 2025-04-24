// Services/Email/IEmailService.cs
namespace YourProjectName.Services.Email
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string verificationToken);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendTwoFactorCodeAsync(string email, string code);
        Task SendGenericEmailAsync(string email, string subject, string htmlContent);
        Task<bool> ValidateEmailAsync(string email);
    }

    public record EmailOptions(
        string FromAddress,
        string FromName,
        string SmtpServer,
        int Port,
        string Username,
        string Password,
        bool EnableSsl = true
    );
}