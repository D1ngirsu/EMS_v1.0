using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.CommandTimeout(120) // Set timeout to 120 seconds
    ));

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Path = "/"; // Explicitly set path
});

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".AspNetCore.AuthCookie"; // Separate from session cookie
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS with credentials support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true); // Allow all origins for LAN
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Add NotificationService
builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseSession(); // Enable session middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.UseRouting();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //context.Database.EnsureDeleted(); // Drops the database if it exists
    context.Database.EnsureCreated(); // Creates the database with the latest schema
}

app.Run();