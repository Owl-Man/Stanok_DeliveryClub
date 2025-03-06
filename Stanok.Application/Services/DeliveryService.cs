using Stanok.Core.Abstractions;
using Stanok.Core.Models;

namespace Stanok.Application.Services;
public class DeliveryService(IDeliveriesRepository deliveriesRepository) : IDeliveryService
{
    public List<Delivery> GetAll()
    {
        return deliveriesRepository.GetAll();
    }

    public Delivery GetDeliveryById(Guid id)
    {
        return deliveriesRepository.GetById(id);
    }

    public Guid Create(Guid id, Guid stanokId)
    {
        return deliveriesRepository.Create(id, stanokId);
    }

    public Guid Update(Guid id, Status status)
    {
        return deliveriesRepository.Update(id, status);
    }
}
