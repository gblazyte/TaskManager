using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Features.Auth;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}


public class Login : Endpoint<LoginRequest>
{
    private readonly SignInManager<IdentityUser> _loginManager;
    private readonly UserManager<IdentityUser> _userManager;

    public Login(SignInManager<IdentityUser> loginManager, UserManager<IdentityUser> userManager)
    {
        _loginManager = loginManager;
        _userManager = userManager;
    }


    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Username);
        if (user == null)
        {
            await SendUnauthorizedAsync(ct); // 401
        }
        
        var loginResult = await _loginManager.PasswordSignInAsync(req.Username, req.Password, false, false);

        if (!loginResult.Succeeded)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }
        
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };
        
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        //responsible for logging in 
        await CookieAuth.SignInAsync(u =>
        {
            u.Roles.AddRange(roles);
            u.Claims.AddRange(claims);
        });

        await SendAsync("Login successful", 200, ct);

    }
}