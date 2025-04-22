// Controllers/AuthController.cs
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<AuthResult> LoginAsync(string usernameOrEmail, string password)
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // بررسی حملات Brute Force
        if (await _bruteForceService.IsBruteForceAttempt())
        {
            await _bruteForceService.RecordFailedAttempt(usernameOrEmail);
            throw new AppException("تعداد تلاش‌های ناموفق زیاد بوده است. لطفا 15 دقیقه دیگر تلاش کنید");
        }

        var player = await _context.Players
            .FirstOrDefaultAsync(x => x.Username == usernameOrEmail || x.Email == usernameOrEmail);

        if (player == null || !BCrypt.Net.BCrypt.Verify(password, player.PasswordHash))
        {
            await _bruteForceService.RecordFailedAttempt(usernameOrEmail);
            throw new AppException("نام کاربری یا رمز عبور نادرست است");
        }

        if (player.FailedLoginAttempts >= 5 && player.LockoutEnd > DateTime.UtcNow)
            throw new AccountLockedException();

        if (!VerifyPassword(password, player.PasswordHash))
        {
            player.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
            throw new AuthFailedException();
        }
        // در صورت موفقیت‌آمیز بودن
        await _bruteForceService.RecordSuccessfulAttempt(usernameOrEmail, ipAddress);

        // بقیه کدهای لاگین...
        var result = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
        return Ok(result);
    }
}