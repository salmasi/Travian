using System.Threading.Tasks;
using YourProjectName.Domain.Entities;

namespace YourProjectName.Services.Security
{
    public interface IBruteForceProtectionService
    {
        // مدیریت تلاش‌های ناموفق
        Task<BruteForceProtectionResult> RecordFailedAttemptAsync(string ipAddress, string username);
        Task ResetFailedAttemptsAsync(string ipAddress, string username);
        
        // بررسی محدودیت‌ها
        Task<bool> IsIpBlockedAsync(string ipAddress);
        Task<bool> IsAccountLockedAsync(string username);
        
        // مدیریت قفل حساب
        Task<AccountLockoutStatus> GetAccountLockoutStatusAsync(string username);
        Task TemporarilyLockAccountAsync(string username, int minutes);
        
        // گزارش‌گیری
        Task<int> GetFailedAttemptsCountAsync(string ipAddress, string username);
        Task<BruteForceStats> GetBruteForceStatisticsAsync();
    }

    public record BruteForceProtectionResult
    {
        public bool IsBlocked { get; init; }
        public int RemainingAttempts { get; init; }
        public int LockoutMinutes { get; init; }
    }

    public record AccountLockoutStatus
    {
        public bool IsLocked { get; init; }
        public DateTime? LockoutEnd { get; init; }
        public int FailedAttempts { get; init; }
    }

    public record BruteForceStats
    {
        public int TotalFailedAttempts { get; init; }
        public int CurrentlyLockedAccounts { get; init; }
        public int CurrentlyBlockedIps { get; init; }
    }
}