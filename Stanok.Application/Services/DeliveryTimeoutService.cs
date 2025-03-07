using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;
using Stanok.DataAccess;
using System.Threading.Tasks;

namespace Stanok.Application.Services;

public class DeliveryTimeoutService : BackgroundService, IDisposable, IDeliveryTimeoutService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ILogger<DeliveryTimeoutService> _logger;

    public const int MAX_STATUS_IGNORE_TIME = 10;
    private readonly Dictionary<Guid, Timer> _timers;

    public DeliveryTimeoutService(IServiceScopeFactory scopeFactory, ILogger<DeliveryTimeoutService> logger)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _timers = new Dictionary<Guid, Timer>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => RestoreTimers());
        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task StartTimerForNewDelivery(IDeliveryService deliveryService, Guid deliveryId)
    {
        try
        {
            _logger.LogInformation("Starting delayed task execution...");

            var timer = new Timer(
                _ => CheckDeliveryStatus(deliveryService, deliveryId),
                null,
                MAX_STATUS_IGNORE_TIME,
                Timeout.Infinite
            );

            _timers[deliveryId] = timer;

            /*_ = Task.Run(async () =>
            {
                await Task.Delay(MAX_STATUS_IGNORE_TIME);
                CheckDeliveryStatus(deliveryService, deliveryId);
            });*/

            _logger.LogInformation("Delayed task completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing delayed task");
        }

        return Task.CompletedTask;
    }

    private void CheckDeliveryStatus(IDeliveryService deliveryService, Guid deliveryId)
    {
        var delivery = deliveryService.GetDeliveryById(deliveryId);
        if (delivery.Status == Status.CREATE
            && delivery.CreatedAt.AddSeconds(MAX_STATUS_IGNORE_TIME) > DateTime.UtcNow)
        {
            deliveryService.Update(deliveryId, Status.CANCELED);
        }
    }

    private async Task RestoreTimers() 
    {
        using var scope = _scopeFactory.CreateScope();

         var context = scope.ServiceProvider.GetRequiredService<StanokDbContext>();

        var deliveries = await context.Deliveries
            .Where(d => d.Status == Status.CREATE)
            .ToListAsync();

        foreach (var delivery in deliveries)
        {
            var deliveryService = scope.ServiceProvider.GetRequiredService<IDeliveryService>();
            await StartTimerForNewDelivery(deliveryService, delivery.Id);
        }
    }
}
