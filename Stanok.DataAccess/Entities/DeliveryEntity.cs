using Stanok.Core.Models;

namespace Stanok.DataAccess.Entities;

public class DeliveryEntity
{
    public Guid Id { get; set; }
    public Guid StanokId { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
}