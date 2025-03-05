using Microsoft.AspNetCore.Mvc;
using Stanok.Core.Abstractions;

namespace Stanok_DeliveryClub.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveriesController(IDeliveryService deliveriesService) : ControllerBase
{
}
