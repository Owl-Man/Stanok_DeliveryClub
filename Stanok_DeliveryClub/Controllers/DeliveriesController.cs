using Microsoft.AspNetCore.Mvc;
using Stanok.Application.Services;
using Stanok.Core.Abstractions;
using Stanok.Core.Models;
using Stanok_DeliveryClub.Contracts;

namespace Stanok_DeliveryClub.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveriesController(IDeliveryService deliveryService) : ControllerBase
{
    [HttpPut("delivery.status.change")]
    public ActionResult<DeliveryResponse> ChangeDeliveryStatus([FromBody] DeliveryRequest request)
    {
        var delivery = new Delivery(Guid.NewGuid(), request.id, request.status);

        var deliveryId = deliveryService.Update(delivery.Id, delivery.Status);

        var response = new DeliveryResponse(deliveryId, request.id, request.status);

        return Ok(response);
    }
}
