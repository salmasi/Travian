namespace YourProjectName.Services;

// در فایل Middleware/BruteForceMiddleware.cs
public class BruteForceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public BruteForceMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        IBruteForceProtectionService bruteForceService)
    {
        var path = context.Request.Path.Value?.ToLower();
        var isLoginPath = path == "/api/auth/login";

        if (isLoginPath && await bruteForceService.IsBruteForceAttempt())
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        await _next(context);
    }
}

// در Program.cs قبل از app.MapControllers():
