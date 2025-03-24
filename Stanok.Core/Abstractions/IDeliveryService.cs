using Stanok.Core.Models;

namespace Stanok.Core.Abstractions
{
    public interface IDeliveryService
    {
        List<Delivery> GetAll();
        Guid Create(Guid stanokId);
        Delivery GetDeliveryById(Guid id);
        Guid Update(Guid id, Status status);
    }
}