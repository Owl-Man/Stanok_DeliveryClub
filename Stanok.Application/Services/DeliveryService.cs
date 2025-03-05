using Stanok.Core.Abstractions;
using Stanok.Core.Models;

namespace Stanok.Application.Services;
public class DeliveryService : IDeliveryService
{
    private readonly IDeliveriesRepository _deliveriesRepository;

    public DeliveryService(IDeliveriesRepository deliveriesRepository)
    {
        _deliveriesRepository = deliveriesRepository;
    }

    public Delivery GetDeliveryById(Guid id)
    {
        return _deliveriesRepository.GetById(id);
    }

    public Guid Create(Guid id, Guid stanokId)
    {
        return _deliveriesRepository.Create(id, stanokId);
    }

    public Guid Update(Guid id, Status status)
    {
        return _deliveriesRepository.Update(id, status);
    }
}
