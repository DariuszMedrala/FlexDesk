using FlexDesk.Domain.Enums;
using FlexDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlexDesk.Infrastructure.Scheduling;

public class ReservationMaintenanceService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan PaymentWindow = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ReservationMaintenanceService> _logger;

    public ReservationMaintenanceService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IHostEnvironment environment,
        ILogger<ReservationMaintenanceService> logger)
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
                await RunAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Reservation maintenance tick failed");
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

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlexDeskDbContext>();

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var unpaid = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.StartTime <= now.Add(PaymentWindow))
            .ToListAsync(cancellationToken);

        foreach (var reservation in unpaid)
        {
            reservation.Status = ReservationStatus.Cancelled;
        }

        var finished = await context.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed && r.EndTime <= now)
            .ToListAsync(cancellationToken);

        foreach (var reservation in finished)
        {
            reservation.Status = ReservationStatus.Completed;
        }

        if (unpaid.Count == 0 && finished.Count == 0)
        {
            return;
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Reservation maintenance: cancelled {Cancelled} unpaid, completed {Completed} finished",
            unpaid.Count,
            finished.Count);
    }
}
