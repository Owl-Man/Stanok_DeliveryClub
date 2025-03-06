using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;

namespace Stanok.Application.Services;

public class DeliveryTimeoutService(IDeliveriesRepository deliveriesRepository, ILogger<DeliveryTimeoutService> logger) : BackgroundService, IDeliveryTimeoutService
{
    public const int MAX_STATUS_IGNORE_TIME = 10;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Task StartTimerForNewDelivery(Guid deliveryId)
    {
        try
        {
            logger.LogInformation("Starting delayed task execution...");

            _ = Task.Run(async () =>
            {
                await Task.Delay(MAX_STATUS_IGNORE_TIME);
                CheckDeliveryStatus(deliveryId);
            });

            logger.LogInformation("Delayed task completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while executing delayed task");
        }

        return Task.CompletedTask;
    }

    private void CheckDeliveryStatus(Guid deliveryId)
    {
        if (deliveriesRepository.GetById(deliveryId).Status == Status.CREATE)
        {
            deliveriesRepository.Update(deliveryId, Status.CANCELED);
        }
    }
}
