
using Stanok.Core.Abstractions;

namespace Stanok.Application.Services
{
    public interface IDeliveryTimeoutService
    {
        Task StartTimerForNewDelivery(Guid deliveryId);
        Task StartTimerForNewDelivery(Guid deliveryId, TimeSpan timeout);
        void DisposeTimerForDelivery(Guid deliveryId);
        void DisposeAll();
    }
}