// Services/Email/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using YourProjectName.Models.EmailTemplates;

namespace YourProjectName.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }


        private readonly IViewRenderer _viewRenderer;

        public async Task SendVerificationEmailAsync(string email, string verificationToken)
        {
            var model = new VerifyAccountModel
            {
                Username = email.Split('@')[0],
                VerificationLink = $"https://yourdomain.com/verify?token={verificationToken}"
            };

            var htmlContent = await _viewRenderer.RenderViewToStringAsync(
                "Views/VerifyAccountTemplate.cshtml",
                model
            );

            await SendGenericEmailAsync(email, "تایید حساب کاربری", htmlContent);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var resetLink = $"https://yourdomain.com/reset-password?token={resetToken}&email={email}";
            var htmlContent = $"""
                <html>
                    <body>
                        <h1>بازیابی رمز عبور</h1>
                        <p>برای تنظیم مجدد رمز عبور روی لینک زیر کلیک کنید:</p>
                        <a href="{resetLink}">تنظیم رمز عبور جدید</a>
                        <p>این لینک تا 1 ساعت معتبر است</p>
                    </body>
                </html>
                """;

            await SendGenericEmailAsync(
                email,
                "بازیابی رمز عبور",
                htmlContent
            );
        }

        public async Task SendGenericEmailAsync(string email, string subject, string htmlContent)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = subject;

                message.Body = new TextPart(TextFormat.Html)
                {
                    Text = htmlContent
                };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _options.SmtpServer,
                    _options.Port,
                    _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
                );

                await client.AuthenticateAsync(_options.Username, _options.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw;
            }
        }

        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                var address = new MailboxAddress("test", email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // پیاده‌سازی سایر متدها...
    }
}