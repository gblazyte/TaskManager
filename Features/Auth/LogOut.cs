using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TaskManager.Features.Auth;


public class LogOut : EndpointWithoutRequest
{
    
    public override void Configure()
    {
        Post("/logout");
        AuthSchemes(CookieAuthenticationDefaults.AuthenticationScheme);

    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        await CookieAuth.SignOutAsync();
        await SendAsync("Logout successful", 200, ct);

    }


}