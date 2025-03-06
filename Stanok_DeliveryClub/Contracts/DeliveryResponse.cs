using Stanok.Core.Models;

namespace Stanok_DeliveryClub.Contracts;

public record DeliveryResponse(Guid id, Guid stanokId, Status status);
