namespace YourProjectName.Models
{
    #region مدل‌های ثبت‌نام و ورود
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? VerificationToken { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? Expires { get; set; }
        public string? UserId { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
    #endregion

    #region مدل‌های مدیریت رمز عبور
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
    #endregion

    #region مدل‌های مدیریت حساب
    public class UpdateProfileRequest
    {
        [StringLength(50)]
        public string? DisplayName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }
    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }
    #endregion
}