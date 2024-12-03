using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using TaskManager.Data;

var builder = WebApplication.CreateBuilder(args);


// add fastendpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();



//database conn
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlite("Data Source=TaskManager.db");
    
});

//login
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// builder.Services.AddAuthenticationCookie(validFor:TimeSpan.FromDays(30), options =>
// {
//     options.Cookie.SameSite = SameSiteMode.None;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
// });

// Configure Cookie Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "TaskManagerAuth";  // Ensure your cookie has a name
        options.Cookie.SameSite = SameSiteMode.Lax;  // Adjust SameSite mode
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure the cookie is always secure
        options.LoginPath = "/login"; // Path to redirect for unauthorized users
        options.LogoutPath = "/logout"; // Path to log out
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Expiry time for the cookie
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
    
    await SeedData.Initialize(userManager,roleManager);
    
}

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();