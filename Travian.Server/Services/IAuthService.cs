// Services/Auth/IAuthService.cs
using System.Security.Claims;
using YourProjectName.Models;

namespace YourProjectName.Services.Auth
{
    public interface IAuthService
    {
        // عملیات پایه
        Task<AuthResult> RegisterPlayerAsync(PlayerRegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task LogoutAsync(string refreshToken);
        Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
        
        // مدیریت حساب کاربری
        Task RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        
        // احراز هویت پیشرفته
        Task<string> GenerateJwtToken(Player player);
        Task<bool> ValidateTokenAsync(string token);
        Task<Player> GetPlayerFromTokenAsync(string token);
        
        // مدیریت نشست‌ها
        Task<IEnumerable<ActiveSession>> GetActiveSessionsAsync(string playerId);
        Task RevokeSessionAsync(string sessionId);
        
        // اعتبارسنجی
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
        
        // لاگین دو مرحله‌ای
        Task<bool> EnableTwoFactorAsync(string playerId, string provider);
        Task<AuthResult> TwoFactorSignInAsync(string code, string provider);
    }

   
}