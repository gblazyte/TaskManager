using Microsoft.AspNetCore.Identity;

namespace TaskManager.Data;

public static class SeedData
{
    public static async Task Initialize(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRoles(roleManager);
        await SeedUsers(userManager);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };

        foreach (string roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedUsers(UserManager<IdentityUser> userManager)
    {
        string userName = "admin@gmail.com";
        string password = "Adminpass123@";

        if (await userManager.FindByNameAsync(userName) == null)
        {
            var user = new IdentityUser
            {
                UserName = userName,
                Email = userName,
                EmailConfirmed = true

            };
            
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
            
            
        }
        
    }
}