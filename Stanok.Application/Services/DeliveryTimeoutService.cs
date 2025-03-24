using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;
using Stanok.DataAccess;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Stanok.Application.Services;

public class DeliveryTimeoutService : BackgroundService, IHostedService, IDisposable, IDeliveryTimeoutService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ILogger<DeliveryTimeoutService> _logger;

    public readonly TimeSpan MAX_STATUS_IGNORE_TIME = TimeSpan.FromSeconds(10);

    private readonly Dictionary<Guid, Timer> _timers;

    public DeliveryTimeoutService(IServiceScopeFactory scopeFactory, ILogger<DeliveryTimeoutService> logger)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _timers = new Dictionary<Guid, Timer>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //Task.Run(() => RestoreTimers());

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            await RestoreTimers();
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
        }

        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public void DisposeTimerForDelivery(Guid deliveryId)
    {
        if (_timers.ContainsKey(deliveryId))
        {
            _timers[deliveryId].Dispose();
            _timers.Remove(deliveryId);
        }
    }

    public void DisposeAll() 
    {
        foreach (var timer in _timers.Values)
        {
            timer.Dispose();
            _logger.LogInformation("Disposed " + timer.ToString());
        }
    }

    public Task StartTimerForNewDelivery(Guid deliveryId) => StartTimerForNewDelivery(deliveryId, MAX_STATUS_IGNORE_TIME);

    public Task StartTimerForNewDelivery(Guid deliveryId,  TimeSpan timeout) 
    {
        try
        {
            _logger.LogInformation("Starting delayed task execution...");

            var timer = new Timer(
                _ => HasDeliveryStatusTimedOut(deliveryId),
                null,
                timeout,
                Timeout.InfiniteTimeSpan
            );

            _timers[deliveryId] = timer;

            _logger.LogInformation("Delayed task completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing delayed task");
        }

        return Task.CompletedTask;
    }

    private bool HasDeliveryStatusTimedOut(Guid deliveryId)
    {
        using var scope = _scopeFactory.CreateScope();
        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();

        var delivery = deliveryService.GetDeliveryById(deliveryId);

        if (delivery == null)
        {
            DisposeTimerForDelivery(deliveryId);
            return false;
        }

        if (delivery.Status == Status.CREATE
            && DateTime.UtcNow >= delivery.CreatedAt.AddSeconds(MAX_STATUS_IGNORE_TIME.TotalSeconds))
        {
            deliveryService.Update(deliveryId, Status.CANCELLED);

            DisposeTimerForDelivery(deliveryId);

            return true;
        }

        return false;
    }

    public async Task RestoreTimers() 
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<StanokDbContext>();

        var deliveries = await context.Deliveries
            .Where(d => d.Status == Status.CREATE)
            .ToListAsync();

        var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();

        foreach (var delivery in deliveries)
        {
            if (HasDeliveryStatusTimedOut(delivery.Id)) continue;

            var leftTimeForIgnore = MAX_STATUS_IGNORE_TIME.TotalSeconds - (DateTime.UtcNow - delivery.CreatedAt).TotalSeconds;

            await StartTimerForNewDelivery(delivery.Id, TimeSpan.FromSeconds(leftTimeForIgnore));
        }
    }
}
