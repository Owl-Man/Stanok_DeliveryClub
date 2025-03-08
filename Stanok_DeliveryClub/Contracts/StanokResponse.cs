namespace Stanok_DeliveryClub.Contracts;

public record StanokResponse(Guid id, string name, string manufacturer, double price, Guid deliveryId);