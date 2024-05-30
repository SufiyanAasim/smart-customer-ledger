using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Constants;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Infrastructure.Data.Contexts;
using CustomerLedger.Infrastructure.Data.Seeders;
using CustomerLedger.Infrastructure.Identity;
using CustomerLedger.Infrastructure.Services;
using CustomerLedger.Web.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Configure it via appsettings.json, " +
        "user secrets, or the ConnectionStrings__DefaultConnection environment variable.");

// A fixed server version avoids a live database round-trip during app startup (which
// ServerVersion.AutoDetect would require) — adjust MySqlServerVersion in appsettings.json
// to match your actual MySQL deployment.
var mySqlVersionSetting = builder.Configuration["MySqlServerVersion"] ?? "8.0.36";
var serverVersion = new MySqlServerVersion(new Version(mySqlVersionSetting));

// ReplicaConnection falls back to the primary connection string when unset — this project
// ships a clearly-labeled *simulated* replica (see docs/releases/v5.0.0-Replica.md) rather
// than requiring native MySQL replication to be configured just to demonstrate the
// read/write separation pattern. AddDbContextPool reuses context instances across
// requests instead of allocating one per request — real connection pooling, not just a
// documented intention.
var replicaConnectionString = builder.Configuration.GetConnectionString("ReplicaConnection") ?? connectionString;

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddDbContextPool<ReplicaDbContext>(options =>
    options.UseMySql(replicaConnectionString, serverVersion));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<ApplicationClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerAccountService, CustomerAccountService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInstallmentPlanService, InstallmentPlanService>();
builder.Services.AddScoped<IInstallmentScheduleService, InstallmentScheduleService>();
builder.Services.AddScoped<ICustomerInteractionService, CustomerInteractionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBackupHistoryService, BackupHistoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReconciliationService, ReconciliationService>();
builder.Services.AddScoped<IReplicaHealthService, ReplicaHealthService>();
builder.Services.AddScoped<IReplicaAwareReportingService, ReplicaAwareReportingService>();
builder.Services.AddSingleton<IShardResolver, CustomerLedger.Infrastructure.Sharding.ShardResolver>();
builder.Services.AddScoped<CustomerLedger.Infrastructure.Sharding.IShardDbContextFactory, CustomerLedger.Infrastructure.Sharding.ShardDbContextFactory>();
builder.Services.AddScoped<ICrossShardReportingService, CustomerLedger.Infrastructure.Sharding.CrossShardReportingService>();
builder.Services.AddScoped<IBackupService, CustomerLedger.Infrastructure.Backup.MySqlBackupService>();
builder.Services.AddScoped<IRestoreService, CustomerLedger.Infrastructure.Backup.MySqlRestoreService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddHostedService<OverdueInstallmentBackgroundService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdministratorOnly, policy =>
        policy.RequireRole(Roles.Administrator));

    options.AddPolicy(AuthorizationPolicies.ManagerOrAbove, policy =>
        policy.RequireRole(Roles.Administrator, Roles.BranchManager));

    options.AddPolicy(AuthorizationPolicies.AnyStaffRole, policy =>
        policy.RequireRole(Roles.Administrator, Roles.BranchManager, Roles.Staff));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();

    await RoleSeeder.SeedAsync(roleManager, logger);
    await AdminUserSeeder.SeedAsync(dbContext, userManager, configuration, logger);

    if (app.Environment.IsDevelopment())
    {
        await DevelopmentDataSeeder.SeedAsync(dbContext);
    }
}

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program
{
}
