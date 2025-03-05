using Stanok.Core.Models;

namespace Stanok.Core.Abstractions
{
    public interface IDeliveryService
    {
        Guid Create(Guid id, Guid stanokId);
        Delivery GetDeliveryById(Guid id);
        Guid Update(Guid id, Status status);
    }
}