// در فایل Services/BruteForceProtectionService.cs
public async Task RecordFailedAttempt(string username)
{
    var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    
    await _context.LoginAttempts.AddAsync(new LoginAttempt
    {
        IpAddress = ipAddress,
        Username = username,
        IsSuccessful = false
    });

    // اگر تعداد تلاش‌ها از حد مجاز بیشتر شد
    var recentAttempts = await GetRecentAttempts(ipAddress);
    if (recentAttempts >= _config.GetValue<int>("Security:MaxLoginAttempts"))
    {
        _logger.LogWarning($"Brute force attempt detected from IP: {ipAddress}");
        // می‌توانید اینجا ایمیل هشدار به ادمین ارسال کنید
    }

    await _context.SaveChangesAsync();
}