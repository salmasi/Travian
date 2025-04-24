// Services/Auth/IAuthService.cs
using System.Security.Claims;
using YourProjectName.Models;

namespace YourProjectName.Models
{

    // مدل‌های کمکی
    public record AuthResult(
        bool Success,
        string Token = null,
        string RefreshToken = null,
        DateTime? Expiration = null,
        IEnumerable<string> Errors = null
    );

    public record PlayerRegisterRequest(
        string Username,
        string Email,
        string Password,
        string ConfirmPassword
    );

    public record LoginRequest(
        string UsernameOrEmail,
        string Password,
        bool RememberMe
    );

    public record ResetPasswordRequest(
        string Email,
        string Token,
        string NewPassword
    );

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );

    public record ActiveSession(
        string SessionId,
        string IpAddress,
        string DeviceInfo,
        DateTime LoginTime,
        DateTime LastActivity
    );

}