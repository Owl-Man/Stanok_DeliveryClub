
using Stanok.Core.Abstractions;

namespace Stanok.Application.Services
{
    public interface IDeliveryTimeoutService
    {
        Task StartTimerForNewDelivery(Guid deliveryId);
        void DisposeTimerForDelivery(Guid deliveryId);
        void DisposeAll();
    }
}