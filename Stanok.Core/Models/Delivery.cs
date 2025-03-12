namespace Stanok.Core.Models;

public class Delivery
{
    public Guid Id { get; }
    public Guid StanokId { get; }
    public Status Status { get; }
    public DateTime CreatedAt { get; set; }

    public Delivery(Guid id, Guid stanokId, Status status, DateTime createdAt)
    {
        Id = id;
        StanokId = stanokId;
        Status = status;
        CreatedAt = createdAt;
    }
}

public enum Status
{
    CREATE,
    IN_DELIVERY,
    DELIVERED,
    CANCELLED
}