
using Stanok.Core.Abstractions;

namespace Stanok.Application.Services
{
    public interface IDeliveryTimeoutService
    {
        Task StartTimerForNewDelivery(IDeliveryService deliveryService, Guid deliveryId);
    }
}