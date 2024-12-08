using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using TaskManager.Data;

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Database connection
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlite("Data Source=TaskManager.db");
});

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// Configure Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Set Google as the default challenge scheme
})
.AddCookie(options =>
{
    options.Cookie.Name = "TaskManagerAuth";  // Ensure your cookie has a name
    options.Cookie.SameSite = SameSiteMode.Lax;  // Adjust SameSite mode
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure the cookie is always secure
    options.LoginPath = "/login"; // Path to redirect for unauthorized users
    options.LogoutPath = "/logout"; // Path to log out
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Expiry time for the cookie
})
.AddGoogle(options =>
{
    options.ClientId = "123727385430-nfm3ou59c0oj1pssc47aolem6mkgllrv.apps.googleusercontent.com";  // Replace with your actual Google Client ID
    options.ClientSecret = "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";  // Replace with your actual Google Client Secret
    options.SaveTokens = true; // Optional: Save access token in authentication properties
    options.Scope.Add("email");  // Request email scope
    options.Scope.Add("profile"); // Request profile scope
    options.CallbackPath = "/signin-google";  // Redirect URI after authentication
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    
    context.Database.Migrate();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    await SeedData.Initialize(userManager, roleManager);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
