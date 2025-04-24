using YourProjectName.Infrastructure.Data;
using YourProjectName.Models;
using YourProjectName.Services;
using YourProjectName.Services.Auth;
using YourProjectName.Services.Email;
using YourProjectName.Services.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// در فایل Program.cs
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IBruteForceProtectionService, BruteForceProtectionService>();
builder.Services.AddScoped<IBattleCalculatorService, BattleCalculatorService>();
builder.Services.AddSingleton<IResourceProductionService, ResourceProductionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IViewRenderer, RazorViewRenderer>();
builder.Services.AddRazorPages();
// اضافه کردن MailKit
builder.Services.AddMailKit(config =>
{
    config.UseMailKit(builder.Configuration.GetSection("EmailSettings").Get<EmailOptions>());
});
// ثبت سرویس‌ها
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("YourProjectName.Infrastructure"))
);

// اعمال خودکار مایگریشن‌ها هنگام راه‌اندازی
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Seed داده‌های اولیه در صورت نیاز
    await SeedData.InitializeAsync(dbContext);
}
// تنظیمات Identity
builder.Services.AddIdentityCore<Player>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GameDbContext>();

// تنظیمات احراز هویت JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Secret"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.Configure<BruteForceProtectionSettings>(
    builder.Configuration.GetSection("BruteForceProtectionSettings"));

// ثبت سرویس
builder.Services.AddScoped<IBruteForceProtectionService, BruteForceProtectionService>();
builder.Services.AddMemoryCache(); // برای مدیریت قفل‌ها
// برای سرویس تولید منابع
builder.Services.AddHostedService<ResourceProductionService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors(options =>
    options.WithOrigins("http://localhost:5173") // آدرس فرانت‌اند
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<BruteForceMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
