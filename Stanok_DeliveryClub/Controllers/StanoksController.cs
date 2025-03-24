using Microsoft.AspNetCore.Mvc;
using Stanok.Application.Services;
using Stanok.Core.Abstractions;
using Stanok_DeliveryClub.Contracts;

namespace Stanok_DeliveryClub.Controllers;

[ApiController]
[Route("[controller]")]
public class StanoksController(IStanokService stanokService, IDeliveryService deliveryService, IDeliveryTimeoutService deliveryTimeoutService) : ControllerBase
{
    [HttpPost("stanok.create")]
    public ActionResult<StanokResponse> CreateStanok([FromBody] StanokRequest request)
    {
        var stanokId = stanokService.Create(request.name, request.manufacturer, request.price);

        var deliveryID = deliveryService.Create(stanokId);

        deliveryTimeoutService.StartTimerForNewDelivery(deliveryID);

        var response = new StanokResponse(stanokId, request.name, request.manufacturer, request.price, deliveryID);

        return Ok(response);
    }
}
