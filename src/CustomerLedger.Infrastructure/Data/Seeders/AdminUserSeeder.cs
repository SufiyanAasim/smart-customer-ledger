using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerLedger.Infrastructure.Data.Seeders;

/// <summary>
/// Creates the first Administrator account from configuration (user secrets or environment
/// variables — see SeedAdmin:Email / SeedAdmin:Password in appsettings.Example.json). Never
/// hardcodes credentials. Does nothing if the configuration section is absent, or if an
/// Administrator already exists.
/// </summary>
public static class AdminUserSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation(
                "SeedAdmin:Email / SeedAdmin:Password not configured — skipping administrator seeding.");
            return;
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = configuration["SeedAdmin:FullName"] ?? "System Administrator",
            EmployeeCode = configuration["SeedAdmin:EmployeeCode"] ?? "ADM-0001",
            BranchId = null,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Failed to seed administrator account: {Errors}",
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(admin, Roles.Administrator);
        logger.LogInformation("Seeded initial administrator account {Email}.", email);
    }
}
