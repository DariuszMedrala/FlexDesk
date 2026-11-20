using FlexDesk.Domain.Enums;
using FlexDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlexDesk.Infrastructure.Scheduling;

public class ReservationExpiryService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan PaymentWindow = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ReservationExpiryService> _logger;

    public ReservationExpiryService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IHostEnvironment environment,
        ILogger<ReservationExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_environment.IsEnvironment("Testing"))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CancelUnpaidReservationsAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to auto-cancel unpaid reservations");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CancelUnpaidReservationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlexDeskDbContext>();

        var threshold = _timeProvider.GetUtcNow().UtcDateTime.Add(PaymentWindow);

        var expired = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.StartTime <= threshold)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0)
        {
            return;
        }

        foreach (var reservation in expired)
        {
            reservation.Status = ReservationStatus.Cancelled;
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Auto-cancelled {Count} unpaid reservation(s) within the payment window", expired.Count);
    }
}
