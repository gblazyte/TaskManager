using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Identity;
using Google.Apis.Auth;

namespace TaskManager.Features.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string GoogleAuthToken { get; set; }  // Google Auth Token for Google login
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
            // Handle Google authentication if GoogleAuthToken is provided
            if (!string.IsNullOrEmpty(req.GoogleAuthToken))
            {
                var googleUser = await AuthenticateWithGoogle(req.GoogleAuthToken);
                if (googleUser == null)
                {
                    await SendUnauthorizedAsync(ct); // 401 if Google authentication fails
                    return;
                }

                // Try to find or create the user based on Google information
                var googleAuthenticatedUser = await _userManager.FindByEmailAsync(googleUser.Email);
                if (googleAuthenticatedUser == null)
                {
                    googleAuthenticatedUser = new IdentityUser { UserName = googleUser.Email, Email = googleUser.Email };
                    var result = await _userManager.CreateAsync(googleAuthenticatedUser);
                    if (!result.Succeeded)
                    {
                        await SendUnauthorizedAsync(ct); // 401 if user creation fails
                        return;
                    }
                }

                // Get roles and claims for the user
                var roles = await _userManager.GetRolesAsync(googleAuthenticatedUser);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, googleAuthenticatedUser.UserName),
                    new Claim(ClaimTypes.NameIdentifier, googleAuthenticatedUser.Id)
                };
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                // Sign in the user using Cookie Authentication
                await CookieAuth.SignInAsync(u =>
                {
                    u.Roles.AddRange(roles);
                    u.Claims.AddRange(claims);
                });

                await SendAsync("Google login successful", 200, ct);
                return;
            }

            // If no GoogleAuthToken is provided, handle traditional username/password login
            var traditionalUser = await _userManager.FindByEmailAsync(req.Username);
            if (traditionalUser == null)
            {
                await SendUnauthorizedAsync(ct); // 401 if user not found
                return;
            }

            var loginResult = await _loginManager.PasswordSignInAsync(req.Username, req.Password, false, false);
            if (!loginResult.Succeeded)
            {
                await SendUnauthorizedAsync(ct); // 401 if login fails
                return;
            }

            // Get roles and claims for the user
            var rolesForTraditionalUser = await _userManager.GetRolesAsync(traditionalUser);
            var claimsForTraditionalUser = new List<Claim>
            {
                new Claim(ClaimTypes.Name, traditionalUser.UserName),
                new Claim(ClaimTypes.NameIdentifier, traditionalUser.Id)
            };
            claimsForTraditionalUser.AddRange(rolesForTraditionalUser.Select(role => new Claim(ClaimTypes.Role, role)));

            // Sign in the user
            await CookieAuth.SignInAsync(u =>
            {
                u.Roles.AddRange(rolesForTraditionalUser);
                u.Claims.AddRange(claimsForTraditionalUser);
            });

            await SendAsync("Login successful", 200, ct);
        }

        // Helper method to authenticate with Google
        private async Task<GoogleUserInfo> AuthenticateWithGoogle(string token)
        {
            try
            {
                // Validate Google token and get user info
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                return new GoogleUserInfo
                {
                    Email = payload.Email,
                    Name = payload.Name
                };
            }
            catch (Exception)
            {
                return null;  // Return null if validation fails
            }
        }
    }

    // Simple class to hold Google user info
    public class GoogleUserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
