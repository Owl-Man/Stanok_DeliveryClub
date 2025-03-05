using Stanok.Core.Models;

namespace Stanok.Core.Abstractions
{
    public interface IDeliveriesRepository
    {
        Guid Create(Guid id, Guid stanokId);
        Delivery GetById(Guid id);
        Guid Update(Guid id, Status status);
    }
}