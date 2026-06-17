using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Background;

/// <summary>
/// Periodically recomputes TAT (tat_pct/tat_state) for active leads so overdue cases
/// surface without a request touching them (spec §12). Runs every 30 minutes.
/// </summary>
public sealed class TatRecalculationService(IServiceScopeFactory scopeFactory, ILogger<TatRecalculationService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once at startup, then on the interval.
        await RecomputeAsync(stoppingToken);
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await RecomputeAsync(stoppingToken);
    }

    private async Task RecomputeAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var leads = scope.ServiceProvider.GetRequiredService<ILeadRepository>();
            var clock = scope.ServiceProvider.GetRequiredService<IClock>();
            var updated = await leads.RecomputeTatAsync(clock.UtcNow);
            logger.LogInformation("TAT recompute updated {Count} leads", updated);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "TAT recompute failed");
        }
    }
}
