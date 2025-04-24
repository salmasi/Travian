using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using YourProjectName.Domain.Entities;
using YourProjectName.Infrastructure.Data;
using YourProjectName.Services.Security;

namespace YourProjectName.Services.Security
{
    public class BruteForceProtectionService : IBruteForceProtectionService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly BruteForceProtectionSettings _settings;
        private readonly ILogger<BruteForceProtectionService> _logger;

        public BruteForceProtectionService(
            AppDbContext context,
            IMemoryCache cache,
            IOptions<BruteForceProtectionSettings> settings,
            ILogger<BruteForceProtectionService> logger)
        {
            _context = context;
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<BruteForceProtectionResult> RecordFailedAttemptAsync(string ipAddress, string username)
        {
            // اعتبارسنجی IP
            if (!IPAddress.TryParse(ipAddress, out _))
                throw new ArgumentException("آدرس IP نامعتبر است");

            // ثبت تلاش ناموفق
            var attempt = new LoginAttempt
            {
                IpAddress = ipAddress,
                Username = username,
                IsSuccessful = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.LoginAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();

            // محاسبه تعداد تلاش‌های اخیر
            var recentAttempts = await GetRecentFailedAttemptsAsync(ipAddress, username);
            var isBlocked = CheckIfBlocked(recentAttempts, out var remainingAttempts, out var lockoutMinutes);

            // اعمال محدودیت در صورت نیاز
            if (isBlocked)
            {
                await TemporarilyLockAccountAsync(username, _settings.AccountLockoutMinutes);
                _logger.LogWarning($"Brute force detected - IP: {ipAddress}, Username: {username}");
            }

            return new BruteForceProtectionResult
            {
                IsBlocked = isBlocked,
                RemainingAttempts = remainingAttempts,
                LockoutMinutes = lockoutMinutes
            };
        }

        public async Task<bool> IsIpBlockedAsync(string ipAddress)
        {
            var cacheKey = $"IP_Block_{ipAddress}";
            return _cache.TryGetValue(cacheKey, out _);
        }

        public async Task<bool> IsAccountLockedAsync(string username)
        {
            var status = await GetAccountLockoutStatusAsync(username);
            return status.IsLocked && status.LockoutEnd > DateTime.UtcNow;
        }

        public async Task<AccountLockoutStatus> GetAccountLockoutStatusAsync(string username)
        {
            var cacheKey = $"Account_Lock_{username}";
            if (_cache.TryGetValue(cacheKey, out DateTime? lockoutEnd))
            {
                var attempts = await _context.LoginAttempts
                    .CountAsync(a => a.Username == username && 
                                   !a.IsSuccessful && 
                                   a.CreatedAt > DateTime.UtcNow.AddMinutes(-_settings.FailedAttemptsWindowMinutes));

                return new AccountLockoutStatus
                {
                    IsLocked = true,
                    LockoutEnd = lockoutEnd,
                    FailedAttempts = attempts
                };
            }

            return new AccountLockoutStatus { IsLocked = false };
        }

        public async Task TemporarilyLockAccountAsync(string username, int minutes)
        {
            var lockoutEnd = DateTime.UtcNow.AddMinutes(minutes);
            var cacheKey = $"Account_Lock_{username}";
            
            _cache.Set(cacheKey, lockoutEnd, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes)
            });

            _logger.LogInformation($"Account {username} locked until {lockoutEnd}");
        }

        private async Task<int> GetRecentFailedAttemptsAsync(string ipAddress, string username)
        {
            var windowStart = DateTime.UtcNow.AddMinutes(-_settings.FailedAttemptsWindowMinutes);
            
            return await _context.LoginAttempts
                .CountAsync(a => 
                    (a.IpAddress == ipAddress || a.Username == username) &&
                    !a.IsSuccessful &&
                    a.CreatedAt > windowStart);
        }

        private bool CheckIfBlocked(int failedAttempts, out int remainingAttempts, out int lockoutMinutes)
        {
            remainingAttempts = Math.Max(0, _settings.MaxFailedAttempts - failedAttempts);
            lockoutMinutes = _settings.AccountLockoutMinutes;
            
            return failedAttempts >= _settings.MaxFailedAttempts;
        }

        // سایر متدها با پیاده‌سازی مشابه...
    }

    public class BruteForceProtectionSettings
    {
        public int MaxFailedAttempts { get; set; } = 5;
        public int FailedAttemptsWindowMinutes { get; set; } = 15;
        public int AccountLockoutMinutes { get; set; } = 30;
        public int IpBlockMinutes { get; set; } = 60;
    }
}