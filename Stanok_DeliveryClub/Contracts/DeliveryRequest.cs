using Stanok.Core.Models;

namespace Stanok_DeliveryClub.Contracts;

public record DeliveryRequest(Guid id, Status status, DateTime craetedAt);