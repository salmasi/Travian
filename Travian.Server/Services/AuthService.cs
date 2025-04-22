// Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly GameDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public AuthService(GameDbContext context, IOptions<JwtSettings> jwtSettings, IEmailService emailService)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
    {
        // اعتبارسنجی
        if (await _context.Players.AnyAsync(x => x.Username == username))
            throw new AppException("نام کاربری قبلا ثبت شده است");

        if (await _context.Players.AnyAsync(x => x.Email == email))
            throw new AppException("ایمیل قبلا ثبت شده است");

        // هش کردن رمز عبور
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        // ایجاد کاربر
        var player = new Player
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Salt = salt,
            VerificationToken = Guid.NewGuid()
        };

        await _context.Players.AddAsync(player);
        
        // ایجاد روستای اولیه
        var village = new Village
        {
            PlayerId = player.Id,
            Name = "روستای اصلی",
            X = new Random().Next(0, 100),
            Y = new Random().Next(0, 100)
        };
        await _context.Villages.AddAsync(village);

        await _context.SaveChangesAsync();

        // ارسال ایمیل تایید
        await _emailService.SendVerificationEmail(player.Email, player.VerificationToken.ToString());

        return new AuthResult(true, player.Id.ToString());
    }

    public async Task<AuthResult> LoginAsync(string usernameOrEmail, string password)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(x => x.Username == usernameOrEmail || x.Email == usernameOrEmail);

        if (player == null || !BCrypt.Net.BCrypt.Verify(password, player.PasswordHash))
            throw new AppException("نام کاربری یا رمز عبور نادرست است");

        if (!player.IsVerified)
            throw new AppException("حساب کاربری تایید نشده است");

        // تولید توکن
        var token = GenerateJwtToken(player);
        var refreshToken = GenerateRefreshToken();

        // ذخیره Refresh Token
        player.RefreshToken = refreshToken;
        player.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        player.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new AuthResult(true, player.Id.ToString())
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }
}