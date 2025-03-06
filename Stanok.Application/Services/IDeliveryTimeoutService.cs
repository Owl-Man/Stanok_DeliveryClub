
namespace Stanok.Application.Services
{
    public interface IDeliveryTimeoutService
    {
        Task StartTimerForNewDelivery(Guid deliveryId);
    }
}