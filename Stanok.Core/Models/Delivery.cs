namespace Stanok.Core.Models;

public class Delivery
{
    public Guid Id { get; }
    public Guid StanokId { get; }
    public Status Status { get; }

    public Delivery(Guid id, Guid stanokId, Status status)
    {
        Id = id;
        StanokId = stanokId;
        Status = status;
    }
}

public enum Status
{
    CREATE,
    IN_DELIVERY,
    DELIVERED,
    CANCELED
}