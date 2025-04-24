using Microsoft.AspNetCore.Http;
using System;

namespace YourProjectName.Extensions
{
    public static class HttpResponseExtensions
    {
        public static void SetTokenCookie(this HttpResponse response, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Strict,
                Secure = true, // فقط در HTTPS
                Path = "/api/auth/refresh-token"
            };

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        public static void RemoveTokenCookie(this HttpResponse response)
        {
            response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/api/auth/refresh-token"
            });
        }
    }
}