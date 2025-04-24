// Services/Auth/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YourProjectName.Configurations;
using YourProjectName.Infrastructure.Data;
using YourProjectName.Models;
using YourProjectName.Services.Email;

namespace YourProjectName.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Player> _passwordHasher;
        private readonly IEmailService _emailService;

        public AuthService(
            IOptions<JwtConfig> jwtConfig,
            AppDbContext context,
            IPasswordHasher<Player> passwordHasher,
            IEmailService emailService)
        {
            _jwtConfig = jwtConfig.Value;
            _context = context;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        public async Task<AuthResult> RegisterPlayerAsync(PlayerRegisterRequest request)
        {
            // اعتبارسنجی
            if (request.Password != request.ConfirmPassword)
                return new AuthResult(false, Errors: new[] { "Passwords do not match" });

            if (await _context.Players.AnyAsync(p => p.Username == request.Username))
                return new AuthResult(false, Errors: new[] { "Username already exists" });

            if (await _context.Players.AnyAsync(p => p.Email == request.Email))
                return new AuthResult(false, Errors: new[] { "Email already in use" });

            // ایجاد بازیکن جدید
            var player = new Player
            {
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                VerificationToken = GenerateRandomToken()
            };

            player.PasswordHash = _passwordHasher.HashPassword(player, request.Password);

            await _context.Players.AddAsync(player);
            await _context.SaveChangesAsync();

            // ارسال ایمیل تایید
            await _emailService.SendVerificationEmail(player.Email, player.VerificationToken);

            // ایجاد روستای اولیه
            await CreateStarterVillage(player.Id);

            // ایجاد توکن
            var token = await GenerateJwtToken(player);
            var refreshToken = GenerateRefreshToken();

            player.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // TODO: دریافت IP واقعی
            });

            await _context.SaveChangesAsync();

            return new AuthResult(true, token, refreshToken);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var player = await _context.Players
                .SingleOrDefaultAsync(p => 
                    p.Username == request.UsernameOrEmail || 
                    p.Email == request.UsernameOrEmail);

            if (player == null)
                return new AuthResult(false, Errors: new[] { "Invalid credentials" });

            var result = _passwordHasher.VerifyHashedPassword(
                player, 
                player.PasswordHash, 
                request.Password);

            if (result != PasswordVerificationResult.Success)
                return new AuthResult(false, Errors: new[] { "Invalid credentials" });

            if (!player.IsVerified)
                return new AuthResult(false, Errors: new[] { "Account not verified" });

            var token = await GenerateJwtToken(player);
            var refreshToken = GenerateRefreshToken();

            player.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // TODO: دریافت IP واقعی
            });

            player.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResult(true, token, refreshToken);
        }

        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var playerId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var player = await _context.Players
                .Include(p => p.RefreshTokens)
                .FirstOrDefaultAsync(p => p.Id == Guid.Parse(playerId));

            if (player == null)
                return new AuthResult(false, Errors: new[] { "Invalid token" });

            var storedRefreshToken = player.RefreshTokens
                .FirstOrDefault(rt => rt.Token == refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.IsExpired)
                return new AuthResult(false, Errors: new[] { "Invalid refresh token" });

            var newToken = await GenerateJwtToken(player);
            var newRefreshToken = GenerateRefreshToken();

            player.RefreshTokens.Remove(storedRefreshToken);
            player.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "127.0.0.1" // TODO: دریافت IP واقعی
            });

            await _context.SaveChangesAsync();

            return new AuthResult(true, newToken, newRefreshToken);
        }

        // سایر متدها با پیاده‌سازی مشابه...
        
        private async Task<string> GenerateJwtToken(Player player)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, player.Username),
                new Claim(JwtRegisteredClaimNames.Email, player.Email),
                new Claim("villageId", player.MainVillageId.ToString() ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}