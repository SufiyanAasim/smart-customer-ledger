using CustomerLedger.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CustomerLedger.Infrastructure.Services;

/// <summary>
/// Time does not flip an installment row from Pending to Overdue on its own — no INSERT/
/// UPDATE/DELETE event occurs merely because a due date passed (spec section 12). This
/// background service is the "explicit scheduled mechanism" that performs that transition,
/// running once at startup and then on a fixed interval.
/// </summary>
public class OverdueInstallmentBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueInstallmentBackgroundService> _logger;

    public OverdueInstallmentBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OverdueInstallmentBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var scheduleService = scope.ServiceProvider.GetRequiredService<IInstallmentScheduleService>();
                var updated = await scheduleService.MarkOverdueInstallmentsAsync(stoppingToken);
                if (updated > 0)
                {
                    _logger.LogInformation("Marked {Count} installment schedule row(s) as Overdue.", updated);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to run the overdue-installment sweep.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
