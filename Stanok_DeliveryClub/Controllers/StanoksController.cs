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
        var stanok = new Stanok.Core.Models.Stanok(Guid.NewGuid(), request.name, request.manufacturer, request.price);

        var stanokId = stanokService.Create(stanok.Id, stanok.Name, stanok.Manufacturer, stanok.Price);

        var deliveryID = deliveryService.Create(Guid.NewGuid(), stanokId);

        deliveryTimeoutService.StartTimerForNewDelivery(deliveryService, deliveryID);

        var response = new StanokResponse(stanokId, request.name, request.manufacturer, request.price);

        return Ok(response);
    }
}
