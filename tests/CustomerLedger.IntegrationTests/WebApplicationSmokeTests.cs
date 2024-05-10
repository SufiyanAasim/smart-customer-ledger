using CustomerLedger.DatabaseTests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CustomerLedger.IntegrationTests;

/// <summary>
/// Boots the full ASP.NET Core pipeline (Program.cs, including RoleSeeder/AdminUserSeeder
/// against a real MySQL database) and confirms an anonymous request to a protected page is
/// redirected to Login rather than served, and that Login itself renders successfully.
/// Skipped when MySQL isn't reachable — see MySqlAvailableFactAttribute.
/// </summary>
public class WebApplicationSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebApplicationSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = TestDatabaseSettings.ConnectionString
                });
            });
        });
    }

    [MySqlAvailableFact]
    public async Task AnonymousRequestToDashboard_RedirectsToLogin()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
    }

    [MySqlAvailableFact]
    public async Task LoginPage_RendersSuccessfully()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/Identity/Account/Login");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CustomerLedger", body);
    }
}
