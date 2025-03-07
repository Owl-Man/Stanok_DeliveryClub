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
        var delivery = new Delivery(Guid.NewGuid(), request.id, request.status, request.craetedAt);

        var deliveryId = deliveryService.Update(delivery.Id, delivery.Status);

        var response = new DeliveryResponse(deliveryId, request.id, request.status, request.craetedAt);

        return Ok(response);
    }

    [HttpGet]
    public ActionResult<List<DeliveryResponse>> GetAllDeliveries()
    {
        var deliveries = deliveryService.GetAll();

        var response = deliveries.Select(d => new DeliveryResponse(d.Id, d.StanokId, d.Status, d.CreatedAt));
        return Ok(response);
    }
}
