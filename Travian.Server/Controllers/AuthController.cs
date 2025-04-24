using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YourProjectName.Models;
using YourProjectName.Services;
using YourProjectName.Extensions;
using YourProjectName.Services.Auth;
using YourProjectName.Services.Email;
using YourProjectName.Services.Security;
using Microsoft.AspNetCore.Authorization;

namespace YourProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly IBruteForceProtectionService _bruteForceService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IEmailService emailService,
            BruteForceProtectionService bruteForceService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _emailService = emailService;
            _bruteForceService = bruteForceService;
            _logger = logger;
        }

        #region ثبت‌نام و فعال‌سازی حساب
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                // اعتبارسنجی مدل
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // ثبت کاربر
                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                // ارسال ایمیل فعال‌سازی
                await _emailService.SendVerificationEmailAsync(
                    request.Email,
                    result.VerificationToken!);

                _logger.LogInformation($"User {request.Email} registered successfully");

                return Ok(new
                {
                    Message = "ثبت‌نام موفقیت‌آمیز بود. لطفا ایمیل خود را تایید کنید",
                    result.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(email, token);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                return Ok(new
                {
                    Message = "حساب کاربری با موفقیت فعال شد",
                    result.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification failed");
                return StatusCode(500, "خطای سرور");
            }
        }
        #endregion

        #region ورود و مدیریت توکن
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                // بررسی محدودیت‌های Brute Force
                var protectionResult = await _bruteForceService.RecordFailedAttemptAsync(
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    request.UsernameOrEmail);

                if (protectionResult.IsBlocked)
                {
                    return BadRequest(new
                    {
                        Message = $"حساب شما به مدت {protectionResult.LockoutMinutes} دقیقه قفل شده است",
                        RemainingAttempts = 0
                    });
                }

                var result = await _authService.LoginAsync(request);

                if (!result.Success)
                    return Unauthorized(new { Errors = result.Errors, RemainingAttempts = protectionResult.RemainingAttempts });

                // تنظیم کوکی امن
                Response.SetTokenCookie(result.RefreshToken!);

                _logger.LogInformation($"User {request.Email} logged in");
                await _bruteForceService.ResetFailedAttemptsAsync(
        HttpContext.Connection.RemoteIpAddress?.ToString(),
        request.UsernameOrEmail);
                return Ok(new
                {
                    Token = result.Token,
                    Expires = result.Expires,
                    UserId = result.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (!result.Success)
                    return Unauthorized(new { Errors = result.Errors });

                Response.SetTokenCookie(result.RefreshToken!);

                return Ok(new
                {
                    Token = result.Token,
                    Expires = result.Expires
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken(RevokeTokenRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _authService.RevokeTokenAsync(userId, request.Token);

                return Ok(new { Message = "توکن با موفقیت غیرفعال شد" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token revocation failed");
                return StatusCode(500, "خطای سرور");
            }
        }
        #endregion

        #region مدیریت رمز عبور
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request.Email);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                // ارسال ایمیل بازنشانی
                await _emailService.SendPasswordResetEmailAsync(
                    request.Email,
                    result.ResetToken!);

                _logger.LogInformation($"Password reset requested for {request.Email}");

                return Ok(new
                {
                    Message = "ایمیل بازنشانی رمز عبور ارسال شد",
                    Token = result.ResetToken // فقط برای تست، در تولید نباید برگردانده شود
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset request failed");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                _logger.LogInformation($"Password reset for user {request.Email}");

                return Ok(new { Message = "رمز عبور با موفقیت تغییر یافت" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _authService.ChangePasswordAsync(userId, request);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                _logger.LogInformation($"Password changed for user {userId}");

                return Ok(new { Message = "رمز عبور با موفقیت تغییر یافت" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change failed");
                return StatusCode(500, "خطای سرور");
            }
        }
        #endregion

        #region مدیریت حساب کاربری
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _authService.GetUserByIdAsync(userId);

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user failed");
                return StatusCode(500, "خطای سرور");
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _authService.UpdateProfileAsync(userId, request);

                if (!result.Success)
                    return BadRequest(new { Errors = result.Errors });

                return Ok(new
                {
                    Message = "پروفایل با موفقیت به‌روزرسانی شد",
                    User = result.User
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile update failed");
                return StatusCode(500, "خطای سرور");
            }
        }
        #endregion
    }
}