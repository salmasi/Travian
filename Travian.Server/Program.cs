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
