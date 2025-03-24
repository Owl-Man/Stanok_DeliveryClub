using Stanok.Core.Models;

namespace Stanok.Core.Abstractions
{
    public interface IDeliveriesRepository
    {
        List<Delivery> GetAll();
        Guid Create(Guid stanokId);
        Delivery GetById(Guid id);
        Guid Update(Guid id, Status status);
    }
}