using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;
using Stanok.DataAccess.Entities;

namespace Stanok.DataAccess.Repositories;

public class DeliveriesRepository(StanokDbContext context, ILogger<DeliveriesRepository> logger) : IDeliveriesRepository
{
    public Delivery GetById(Guid id)
    {
        try
        {
            var deliveryEntity = context.Deliveries
                .AsNoTracking()
                .FirstOrDefault(d => d.Id == id);

            Delivery delivery = new Delivery(deliveryEntity.Id, deliveryEntity.StanokId, deliveryEntity.Status);

            return delivery;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении доставки {DeliveryId}.", id);
            throw;
        }
    }

    public Guid Create(Guid id, Guid stanokId)
    {
        try
        {
            var deliveryEntity = new DeliveryEntity() { Id = id, StanokId = stanokId, Status = Status.CREATE };

            context.Deliveries.AddAsync(deliveryEntity);
            context.SaveChangesAsync();

            return deliveryEntity.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании доставки {DeliveryId}.", id);
            throw;
        }
    }

    public Guid Update(Guid id, Status status)
    {
        try
        {
            context.Deliveries
                .Where(d => d.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(d => d.Status, d => status));

            return id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении доставки {DeliveryId}.", id);
            throw;
        }
    }
}
